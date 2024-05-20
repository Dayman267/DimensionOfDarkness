using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IPausable
{
    //[SerializeField] private Image Crosshair;
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
    [SerializeField] private float rotationSpeed = 500f;
    private float currentSpeed = 5f;
    private float targetSpeed = 5f;
    private float speedTransitionDuration = 0.5f;
    private float speedTransitionProgress = 0f;

    private float runShootDeley = 0f;


    private bool isDashing;
    [Header("Dashing")] [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float spendPointsWhenDashing = 10f;

    private bool isVaulting;
    [Header("Vaulting")] [SerializeField] private float vaultSpeed = 6f;
    [SerializeField] private float vaultTime = 0.2f;
    [SerializeField] private float stayVaultRadius = 0.6f;
    [SerializeField] private float spendPointsWhenVaulting = 20f;

    public static event Action<float> OnMoveAnimation;
    public static event Action OnAimAnimationEnable;
    public static event Action OnAimAnimationDiasble;
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

    private bool isPaused = false;

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

    public static void SetPlayerState(PlayerStates state)
    {
        playerState = state;
    }

    public static PlayerStates GetPlayerState()
    {
        return playerState;
    }

    public static PlayerMoveStates GetPlayerMoveState()
    {
        return playerMoveState;
    }

    public static bool IsPlayerHasIdleState()
    {
        return playerState == PlayerStates.idle;
    }

    public static bool IsRightClickDown()
    {
        return isRightClickDown;
    }

    public static bool IsLeftClickDown()
    {
        return isLeftClickDown;
    }

    public static bool IsLShiftDown()
    {
        return isLShiftDown;
    }

    public static bool IsRKeyDown()
    {
        return isRKeyDown;
    }

    public static bool IsQKeyDown()
    {
        return isQKeyDown;
    }

    public static bool IsEKeyDown()
    {
        return isEKeyDown;
    }

    public static bool inMovement;

    void Update()
    {
        if (isPaused) return;
        
        _movementVector = CalculateMovementVector();
        direction = playerActions.Gameplay.Movement.ReadValue<Vector3>();

        inMovement = Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.z) > 0;


        if (inMovement)
        {
            if (playerMoveState != PlayerMoveStates.aiming)
            {
                playerMoveState = PlayerMoveStates.running;
            }
        }
        else
        {
            if(playerMoveState != PlayerMoveStates.aiming)
                playerMoveState = PlayerMoveStates.idle;
        }
        
        // if(!PlayerShootController.IsReloading())
        MoveAnimEnable();

        if (isRightClickDown || isLeftClickDown)
        {
            AimOn();
        }
        else
        {
            AimOff();
        }

        /*f (isRightClickDown)
        {
            AimOn();
        }
        else
        {
            if (!isLeftClickDown)
                AimOff();
        }

        if (isLeftClickDown)
        {
            ShootAnimOn();
        }
        else
        {
            ShootAnimOff();
        }
        */

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


    private void ChangeSpeed()
    {
        if (inMovement && playerMoveState != PlayerMoveStates.aiming)
        {
            targetSpeed = speedRun;
            speedTransitionDuration = 0.1f;
        }
        else if (playerMoveState == PlayerMoveStates.aiming)
        {
            targetSpeed = speedAim;
            speedTransitionDuration = 1f;
        }
        else
        {
            targetSpeed = 0f;
            speedTransitionDuration = 1f;
        }

        if (currentSpeed != targetSpeed)
        {
            speedTransitionProgress += Time.deltaTime / speedTransitionDuration;
            currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, speedTransitionProgress);
        }
    }

    private IEnumerator ChangePlayerMoveStateWithDelay(PlayerMoveStates newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        playerMoveState = newState;
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
        switch (playerActions.Gameplay.W.WasReleasedThisFrame(),
            playerActions.Gameplay.S.WasReleasedThisFrame(),
            playerActions.Gameplay.A.WasReleasedThisFrame(),
            playerActions.Gameplay.D.WasReleasedThisFrame())
        {
            case (true, false, false, false):
                RotateToDirection(Vector3.forward);
                break;
            case (false, true, false, false):
                RotateToDirection(Vector3.back);
                break;
            case (false, false, true, false):
                RotateToDirection(Vector3.left);
                break;
            case (false, false, false, true):
                RotateToDirection(Vector3.right);
                break;
        }
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
        float angle =
            Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void TurnToMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = cam.transform.position.y - transform.position.y; // Устанавливаем Z равным расстоянию от камеры до персонажа
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        // Поворот персонажа курсором мыши с использованием интерполяции
        Quaternion targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
        float rotationSpeed = 10f; // Скорость поворота (можете настроить под свои нужды)

        // Ограничиваем вращение только по оси Y
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
    }

    private void AimOn()
    {
        playerMoveState = PlayerMoveStates.aiming;
        TurnToMousePosition();
        OnAimAnimationEnable?.Invoke();
    }

    private void AimOff()
    {
        playerMoveState = PlayerMoveStates.idle;
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
        
        PauseGame.OnGamePaused += OnPause;
        PauseGame.OnGameResumed += OnResume;
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
        
        PauseGame.OnGamePaused -= OnPause;
        PauseGame.OnGameResumed -= OnResume;
    }

    public void OnPause()
    {
        isPaused = true;
    }

    public void OnResume()
    {
        isPaused = false;
    }
}