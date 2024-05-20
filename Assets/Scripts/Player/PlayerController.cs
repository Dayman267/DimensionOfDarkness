using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    private Camera cam;
    private static Vector3 direction;

    private Vector3 _movementVector;
    private PlayerActions playerActions;
    private static PlayerStates playerState = PlayerStates.idle;
    private static PlayerMoveStates playerMoveState = PlayerMoveStates.idle;


    private bool isAiming = false;
    private bool isRotating;
    private Quaternion targetRotation;

    [Header("Player Speed")] [SerializeField]
    private float speedIncreaseFactor = 1.5f;

    [SerializeField] private float spendPointsWhenRunning = 0.3f;
    [SerializeField] private float speedRun = 10f;
    [SerializeField] private float speedAim = 1.5f;
    [SerializeField] private float speedDash = 15f;
    [SerializeField] private float rotationSpeed = 500f;
    private float currentSpeed = 5f;
    private float targetSpeed = 5f;
    [SerializeField] private float speedTransitionDuration = 0.5f;
    private float speedTransitionProgress = 0f;

    private float runShootDeley = 0f;


    private bool isDashing;
    [Header("Dashing")] [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float spendPointsWhenDashing = 5f;

    private bool isVaulting;
    [Header("Vaulting")] [SerializeField] private float vaultSpeed = 6f;
    [SerializeField] private float vaultTime = 0.2f;
    [SerializeField] private float stayVaultRadius = 0.6f;
    [SerializeField] private float spendPointsWhenVaulting = 20f;
    [SerializeField] private CameraController cameraController;

    public static event Action<float> OnMoveAnimation;
    public static event Action OnAimAnimationEnable;
    public static event Action OnAimAnimationDiasble;
    public static event Action OnDashAnimation;
    public static event Action<float, float> OnSend_X_Z_Pos;

    public float turnSmoothTime = 0.3f;

    public float gravity = -9.81f;

    //public float jumpHeight = 3f;
    CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;
    public float groundDistance = 0.4f;
    float turnSmoothVelocity;
    bool isGrounded;
    Vector3 velocity;
    private float targetAngle;

    private PlayerStamina playerStamina;

    private void Awake()
    {
        cam = Camera.main;

        controller = GetComponent<CharacterController>();
        playerActions = new PlayerActions();
        playerActions.Gameplay.Enable();
    }

    private void Start()
    {
        // #if !UNITY_EDITOR
        //     Application.Quit();
        // #endif
        // #if UNITY_EDITOR
        //     EditorApplication.isPlaying = false;
        // #endif


        playerStamina = GetComponent<PlayerStamina>();

        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    static bool isRightClickDown;
    static bool isLeftClickDown;
    static bool isLShiftDown;
    static bool isRKeyDown;
    static bool isQKeyDown;
    static bool isEKeyDown;
    static bool isSpaceKeyDown;
    public static bool inMovement;

    public static void SetPlayerState(PlayerStates state) => playerState = state;
    public static PlayerStates GetPlayerState() => playerState;
    public static PlayerMoveStates GetPlayerMoveState() => playerMoveState;
    public static bool IsPlayerHasIdleState() => playerState == PlayerStates.idle;
    public static bool IsRightClickDown() => isRightClickDown;
    public static bool IsLeftClickDown() => isLeftClickDown;
    public static bool IsLShiftDown() => isLShiftDown;
    public static bool IsRKeyDown() => isRKeyDown;
    public static bool IsQKeyDown() => isQKeyDown;
    public static bool IsEKeyDown() => isEKeyDown;


    void Update()
    {
        _movementVector = CalculateMovementVector();
        direction = playerActions.Gameplay.Movement.ReadValue<Vector3>();
        inMovement = Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.z) > 0;
        MoveAnimEnable();

        if (isSpaceKeyDown && playerStamina.GetStaminaPoints() >= 30)
        {
            playerStamina.SpendStamina(spendPointsWhenDashing);
            Dash();
        }

        if (playerMoveState != PlayerMoveStates.dashing)
        {
            Debug.Log(playerMoveState);
            if (inMovement)
            {
                if (playerMoveState != PlayerMoveStates.aiming && playerMoveState != PlayerMoveStates.dashing)
                {
                    playerMoveState = PlayerMoveStates.running;
                }
            }
            else
            {
                if (playerMoveState != PlayerMoveStates.aiming)
                    playerMoveState = PlayerMoveStates.idle;
            }

            if (isRightClickDown || isLeftClickDown)
            {
                currentSpeed = speedAim;
                speedTransitionProgress = 0f;

                AimOn();
            }
            else
            {
                AimOff();
            }
        }

        if (!inMovement && !isRightClickDown && !isLeftClickDown)
        {
            ListenWASDKeyUp();
            if (isRotating)
            {
                RotateTowardsTarget();
            }
        }

        if (isDashing || isVaulting) return;

        ChangeSpeed();
        Move(inMovement);
        // if (Vaulting())
        // {
        //     RaycastHit2D hit = 
        //         Physics2D.CircleCast(transform.position, stayVaultRadius, 
        //                         Vector2.zero, Mathf.Infinity, mask);
        //     if (!isVaultable(hit)) return;
        //     StartCoroutine(Vault(hit.collider));
        //     playerStamina.SpendStamina(spendPointsWhenVaulting);
        // }
        // else if (Dashing())
        // {
        //     StartCoroutine(Dash());
        //     playerStamina.SpendStamina(spendPointsWhenDashing);
        // }
        // else if (Running())
        //Sprint(direction, speed, speedIncreaseFactor);
        //playerStamina.SpendStamina(spendPointsWhenRunning);
    }


    private void Dash()
    {
        if (playerMoveState == PlayerMoveStates.dashing) return;

        AimOff();
    
        // Determine the dash direction based on movement or facing direction
        playerMoveState = PlayerMoveStates.dashing;
        Vector3 dashDirection = direction != Vector3.zero ? direction : transform.forward;
        StartCoroutine(PerformDash(dashDirection));
        
        OnDashAnimation?.Invoke();
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        // Calculate the dash direction relative to the camera
        if (dashDirection != transform.forward)
        {
            float targetAngle = Mathf.Atan2(dashDirection.x, dashDirection.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            dashDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }

        while (Time.time < startTime + dashTime)
        {
            RotateCharacterTowardsDashDirection();
            
            controller.Move(dashDirection.normalized * dashSpeed * Time.deltaTime);
            yield return null;
        }
    }
    
    private void RotateCharacterTowardsDashDirection()
    {
        // If there's movement input, rotate towards that direction; otherwise, use current facing direction
        float targetAngle = direction != Vector3.zero ? Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y : transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }
    
    public void EndRoll()
    {
        playerMoveState = PlayerMoveStates.idle;
    }

    private void ChangeSpeed()
    {
        if (inMovement)
        {
            if (playerMoveState != PlayerMoveStates.aiming)
            {
                targetSpeed = speedRun;
            }
            else if (playerMoveState == PlayerMoveStates.aiming)
            {
                targetSpeed = speedAim;
            }
            else if (playerMoveState == PlayerMoveStates.dashing)
            {
                targetSpeed = speedDash;
            }
        }
        else
        {
            currentSpeed = 0f;
            return;
        }

        if (currentSpeed != targetSpeed)
        {
            currentSpeed = Mathf.SmoothDamp(currentSpeed, targetSpeed, ref speedTransitionProgress,
                speedTransitionDuration);
        }
    }

    private void RotateTowardsTarget()
    {
        transform.rotation =
            Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
        {
            isRotating = false;
        }
    }

    private void ListenWASDKeyUp()
    {
        bool wReleased = playerActions.Gameplay.W.WasReleasedThisFrame();
        bool sReleased = playerActions.Gameplay.S.WasReleasedThisFrame();
        bool aReleased = playerActions.Gameplay.A.WasReleasedThisFrame();
        bool dReleased = playerActions.Gameplay.D.WasReleasedThisFrame();

        if (wReleased) RotateToDirection(Vector3.forward);
        if (sReleased) RotateToDirection(Vector3.back);
        if (aReleased) RotateToDirection(Vector3.left);
        if (dReleased) RotateToDirection(Vector3.right);
    }

    private void RotateToDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            isRotating = true;
        }
    }

    private void MoveAnimEnable()
    {
        OnMoveAnimation?.Invoke(_movementVector.magnitude);
    }

    private float maxMovementMagnitude = 1f;

    private Vector3 CalculateMovementVector()
    {
        float h = direction.x;
        float v = direction.z;

        Vector3 cameraR = cam.transform.right;
        Vector3 cameraF = cam.transform.forward;

        cameraR.y = 0;
        cameraF.y = 0;

        if (isLShiftDown)
        {
            maxMovementMagnitude = 1.5f;
        }
        else
        {
            maxMovementMagnitude = 1f;
        }

        Vector3 movementVector = cameraF.normalized * v + cameraR.normalized * h;
        movementVector = Vector3.ClampMagnitude(movementVector, maxMovementMagnitude);

        float targetMagnitude = isLShiftDown && playerStamina.GetStaminaPoints() > 0 ? 1.5f : 1f;
        float lerpedMagnitude = Mathf.MoveTowards(_movementVector.magnitude, targetMagnitude, 1.1f * Time.deltaTime);
        movementVector = movementVector.normalized * lerpedMagnitude;

        Vector3 relativeVector = transform.InverseTransformDirection(movementVector);
        OnSend_X_Z_Pos?.Invoke(relativeVector.x, relativeVector.z);

        return movementVector;
    }

    private void TurnCharacterInMovementDirection()
    {
        if (direction != Vector3.zero)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }


    [SerializeField, Range(0, 10)] private float offset;

    private void TurnToMousePosition()
    {
        Vector3 mousePosition = Mouse.current.position.value;
        mousePosition.z = cam.transform.position.y - transform.position.y;
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        /*Vector3 temp = cam.transform.localPosition;
        temp.x = transform.position.x + (mousePosition.x - 0.5f) * cameraController.offsetX;
        temp.y = transform.position.y + (mousePosition.y - 0.5f) * cameraController.offsetY;
        cam.transform.localPosition = temp;*/


        Quaternion targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
        float rotationSpeed = 10f;

        targetRotation.eulerAngles = new Vector3(0f, targetRotation.eulerAngles.y, 0f);

        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }


    /*private void ResetAngularVelocity()
    {
        rb.angularVelocity = Vector3.zero;
    }*/

    /*private bool IsSprinting() =>
        isLShiftDown && direction != Vector3.zero; // && playerStamina.GetStaminaPoints() > 0;*/

    // private bool Vaulting()
    // {
    //     if (!Input.GetKeyDown(KeyCode.Space) || !(playerStamina.GetStaminaPoints() > 0)) return false;
    //     RaycastHit2D hit =
    //         Physics2D.CircleCast(transform.position, stayVaultRadius,
    //             Vector2.zero, Mathf.Infinity, mask);
    //     return isVaultable(hit);
    // }
    //
    // private bool isVaultable(RaycastHit2D hit) =>
    //     !hit.collider.IsUnityNull() && LayerMask.LayerToName(hit.collider.gameObject.layer) == "Vaultable";
    //
    // private bool Dashing() => 
    //     Input.GetKeyDown(KeyCode.Space) && direction != Vector2.zero && playerStamina.GetStaminaPoints() > 0;

    /*public void Move(Vector3 direction, float speed)
    {
        rb.velocity = direction * speed;
    }*/

    private void Move(bool isMoving)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isMoving)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;

            // Если игрок не прицеливается, поворачиваем его в направлении движения
            if (!isLeftClickDown && !isRightClickDown)
                TurnCharacterInMovementDirection();

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = isLShiftDown && playerStamina.GetStaminaPoints() > 0 &&
                          playerMoveState != PlayerMoveStates.aiming
                ? currentSpeed * speedIncreaseFactor
                : currentSpeed;

            if (isLShiftDown)
                playerStamina.SpendStamina(spendPointsWhenRunning);

            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    /*private void Move(bool isMoving)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        if (isMoving)
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            if (!isLeftClickDown && !isRightClickDown) TurnCharacterInMovementDirection();

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = isLShiftDown &&
                          playerStamina.GetStaminaPoints() > 0 &&
                          playerMoveState != PlayerMoveStates.aiming
                ? currentSpeed * speedIncreaseFactor
                : currentSpeed;
            if (isLShiftDown) playerStamina.SpendStamina(spendPointsWhenRunning);
            controller.Move(moveDir.normalized * speed * Time.deltaTime);
        }

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);

        // if (Input.GetButtonDown("Jump") && isGrounded)
        // {
        //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }
    }*/

    private void AimOn()
    {
        playerMoveState = PlayerMoveStates.aiming;
        if(playerMoveState != PlayerMoveStates.dashing)
            TurnToMousePosition();
        OnAimAnimationEnable?.Invoke();
    }

    private void AimOff()
    {
        playerMoveState = PlayerMoveStates.idle;
        if(playerMoveState != PlayerMoveStates.dashing)
            TurnCharacterInMovementDirection();
        OnAimAnimationDiasble?.Invoke();
    }

    private void OnEnable()
    {
        playerActions.Gameplay.Enable();
        playerActions.Gameplay.Aim.performed += ctx => isRightClickDown = true;
        playerActions.Gameplay.Aim.canceled += ctx => isRightClickDown = false;

        playerActions.Gameplay.Shoot.performed += ctx => isLeftClickDown = true;
        playerActions.Gameplay.Shoot.canceled += ctx => isLeftClickDown = false;

        playerActions.Gameplay.Sprint.performed += ctx => isLShiftDown = true;
        playerActions.Gameplay.Sprint.canceled += ctx => isLShiftDown = false;

        playerActions.Gameplay.WeaponReload.performed += ctx => isRKeyDown = true;
        playerActions.Gameplay.WeaponReload.canceled += ctx => isRKeyDown = false;

        playerActions.Gameplay.ChangeGunBackward.performed += ctx => isQKeyDown = true;
        playerActions.Gameplay.ChangeGunBackward.canceled += ctx => isQKeyDown = false;

        playerActions.Gameplay.ChangeGunForward.performed += ctx => isEKeyDown = true;
        playerActions.Gameplay.ChangeGunForward.canceled += ctx => isEKeyDown = false;

        playerActions.Gameplay.Dash.performed += ctx => isSpaceKeyDown = true;
        playerActions.Gameplay.Dash.canceled += ctx => isSpaceKeyDown = false;
    }

    private void OnDisable()
    {
        playerActions.Gameplay.Disable();
        playerActions.Gameplay.Aim.performed -= ctx => isRightClickDown = true;
        playerActions.Gameplay.Aim.canceled -= ctx => isRightClickDown = false;

        playerActions.Gameplay.Shoot.performed -= ctx => isLeftClickDown = true;
        playerActions.Gameplay.Shoot.canceled -= ctx => isLeftClickDown = false;

        playerActions.Gameplay.Sprint.performed -= ctx => isLShiftDown = true;
        playerActions.Gameplay.Sprint.canceled -= ctx => isLShiftDown = false;

        playerActions.Gameplay.WeaponReload.performed -= ctx => isRKeyDown = true;
        playerActions.Gameplay.WeaponReload.canceled -= ctx => isRKeyDown = false;

        playerActions.Gameplay.ChangeGunBackward.performed -= ctx => isQKeyDown = true;
        playerActions.Gameplay.ChangeGunBackward.canceled -= ctx => isQKeyDown = false;

        playerActions.Gameplay.ChangeGunForward.performed -= ctx => isEKeyDown = true;
        playerActions.Gameplay.ChangeGunForward.canceled -= ctx => isEKeyDown = false;

        playerActions.Gameplay.Dash.performed -= ctx => isSpaceKeyDown = true;
        playerActions.Gameplay.Dash.canceled -= ctx => isSpaceKeyDown = false;
    }
}