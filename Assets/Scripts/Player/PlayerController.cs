using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour, IPausable
{
    private Camera cam;
    private static Vector3 _direction;

    private Vector3 movementVector;
    private PlayerActions playerActions;
    private static PlayerStates _playerState = PlayerStates.idle;
    private static PlayerMoveStates _playerMoveState = PlayerMoveStates.idle;


    //private bool isAiming = false;
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
    
    private bool isPaused = false;
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

    private static bool _isRightClickDown;
    private static bool _isLeftClickDown;
    private static bool _isLShiftDown;
    private static bool _isRKeyDown;
    private static bool _isQKeyDown;
    private static bool _isEKeyDown;
    private static bool _isSpaceKeyDown;
    public static bool inMovement;

    public static void SetPlayerState(PlayerStates state) => _playerState = state;
    public static PlayerStates GetPlayerState() => _playerState;
    public static PlayerMoveStates GetPlayerMoveState() => _playerMoveState;
    public static bool IsPlayerHasIdleState() => _playerState == PlayerStates.idle;
    public static bool IsRightClickDown() => _isRightClickDown;
    public static bool IsLeftClickDown() => _isLeftClickDown;
    public static bool IsLShiftDown() => _isLShiftDown;
    public static bool IsRKeyDown() => _isRKeyDown;
    public static bool IsQKeyDown() => _isQKeyDown;
    public static bool IsEKeyDown() => _isEKeyDown;


    void Update()
    {
        if(isPaused) return;
        
        movementVector = CalculateMovementVector();
        _direction = playerActions.Gameplay.Movement.ReadValue<Vector3>();
        inMovement = Mathf.Abs(_direction.x) > 0 || Mathf.Abs(_direction.z) > 0;
        MoveAnimEnable();

        if (_isSpaceKeyDown && playerStamina.GetStaminaPoints() >= 30)
        {
            playerStamina.SpendStamina(spendPointsWhenDashing);
            Dash();
        }

        if (_playerMoveState != PlayerMoveStates.dashing)
        {
            if (inMovement)
            {
                if (_playerMoveState != PlayerMoveStates.aiming && _playerMoveState != PlayerMoveStates.dashing)
                {
                    _playerMoveState = PlayerMoveStates.running;
                }
            }
            else
            {
                if (_playerMoveState != PlayerMoveStates.aiming)
                    _playerMoveState = PlayerMoveStates.idle;
            }

            if (_isRightClickDown || _isLeftClickDown)
            {
                currentSpeed = speedAim;
                speedTransitionProgress = 0f;

                AimOn();

                if(cameraController != null)
                    cameraController.LookAround(Mouse.current.position.value);
            }
            else
            {
                AimOff();
                if(cameraController != null)
                    cameraController.StopLookingAround();   
            }
        }

        if (!inMovement && !_isRightClickDown && !_isLeftClickDown)
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
        if (_playerMoveState == PlayerMoveStates.dashing) return;

        AimOff();

        // Determine the dash direction based on movement or facing direction
        _playerMoveState = PlayerMoveStates.dashing;
        Vector3 dashDirection = _direction != Vector3.zero ? _direction : transform.forward;
        StartCoroutine(PerformDash(dashDirection));

        OnDashAnimation?.Invoke();
    }

    private IEnumerator PerformDash(Vector3 dashDirection)
    {
        float startTime = Time.time;
        // Calculate the dash direction relative to the camera
        if (dashDirection != transform.forward)
        {
            float targetAngle = Mathf.Atan2(dashDirection.x, dashDirection.z) * Mathf.Rad2Deg +
                                cam.transform.eulerAngles.y;
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
        float targetAngle = _direction != Vector3.zero
            ? Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y
            : transform.eulerAngles.y;
        float angle =
            Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    public void EndRoll()
    {
        currentSpeed = 1f;
        _playerMoveState = PlayerMoveStates.idle;
    }

    private void ChangeSpeed()
    {
        if (inMovement)
        {
            if (_playerMoveState != PlayerMoveStates.aiming)
            {
                targetSpeed = speedRun;
            }
            else if (_playerMoveState == PlayerMoveStates.aiming)
            {
                targetSpeed = speedAim;
            }
            else if (_playerMoveState == PlayerMoveStates.dashing)
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
        OnMoveAnimation?.Invoke(movementVector.magnitude);
    }

    private float maxMovementMagnitude = 1f;

    private Vector3 CalculateMovementVector()
    {
        float h = _direction.x;
        float v = _direction.z;

        Vector3 cameraR = cam.transform.right;
        Vector3 cameraF = cam.transform.forward;

        cameraR.y = 0;
        cameraF.y = 0;

        if (_isLShiftDown)
        {
            maxMovementMagnitude = 1.5f;
        }
        else
        {
            maxMovementMagnitude = 1f;
        }

        Vector3 movementVector = cameraF.normalized * v + cameraR.normalized * h;
        movementVector = Vector3.ClampMagnitude(movementVector, maxMovementMagnitude);

        float targetMagnitude = _isLShiftDown && playerStamina.GetStaminaPoints() > 0 ? 1.5f : 1f;
        float lerpedMagnitude =
            Mathf.MoveTowards(this.movementVector.magnitude, targetMagnitude, 1.1f * Time.deltaTime);
        movementVector = movementVector.normalized * lerpedMagnitude;

        Vector3 relativeVector = transform.InverseTransformDirection(movementVector);
        OnSend_X_Z_Pos?.Invoke(relativeVector.x, relativeVector.z);

        return movementVector;
    }

    private void TurnCharacterInMovementDirection()
    {
        if (_direction != Vector3.zero)
        {
            targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity,
                turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
        }
    }

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
            targetAngle = Mathf.Atan2(_direction.x, _direction.z) * Mathf.Rad2Deg + cam.transform.eulerAngles.y;

            // Если игрок не прицеливается, поворачиваем его в направлении движения
            if (!_isLeftClickDown && !_isRightClickDown)
                TurnCharacterInMovementDirection();

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float speed = _isLShiftDown && playerStamina.GetStaminaPoints() > 0 &&
                          _playerMoveState != PlayerMoveStates.aiming
                ? currentSpeed * speedIncreaseFactor
                : currentSpeed;

            if (_isLShiftDown)
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
        _playerMoveState = PlayerMoveStates.aiming;
        if (_playerMoveState != PlayerMoveStates.dashing)
            TurnToMousePosition();
        OnAimAnimationEnable?.Invoke();
    }

    private void AimOff()
    {
        _playerMoveState = PlayerMoveStates.idle;
        if (_playerMoveState != PlayerMoveStates.dashing)
            TurnCharacterInMovementDirection();
        OnAimAnimationDiasble?.Invoke();
    }

    private void OnEnable()
    {
        PauseGame.OnGamePaused += OnPause;
        PauseGame.OnGameResumed += OnResume;
        
        playerActions.Gameplay.Enable();
        playerActions.Gameplay.Aim.performed += ctx => _isRightClickDown = true;
        playerActions.Gameplay.Aim.canceled += ctx => _isRightClickDown = false;

        playerActions.Gameplay.Shoot.performed += ctx => _isLeftClickDown = true;
        playerActions.Gameplay.Shoot.canceled += ctx => _isLeftClickDown = false;

        playerActions.Gameplay.Sprint.performed += ctx => _isLShiftDown = true;
        playerActions.Gameplay.Sprint.canceled += ctx => _isLShiftDown = false;

        playerActions.Gameplay.WeaponReload.performed += ctx => _isRKeyDown = true;
        playerActions.Gameplay.WeaponReload.canceled += ctx => _isRKeyDown = false;

        playerActions.Gameplay.ChangeGunBackward.performed += ctx => _isQKeyDown = true;
        playerActions.Gameplay.ChangeGunBackward.canceled += ctx => _isQKeyDown = false;

        playerActions.Gameplay.ChangeGunForward.performed += ctx => _isEKeyDown = true;
        playerActions.Gameplay.ChangeGunForward.canceled += ctx => _isEKeyDown = false;

        playerActions.Gameplay.Dash.performed += ctx => _isSpaceKeyDown = true;
        playerActions.Gameplay.Dash.canceled += ctx => _isSpaceKeyDown = false;
    }

    private void OnDisable()
    {
        PauseGame.OnGamePaused -= OnPause;
        PauseGame.OnGameResumed -= OnResume;
        
        playerActions.Gameplay.Disable();
        playerActions.Gameplay.Aim.performed -= ctx => _isRightClickDown = true;
        playerActions.Gameplay.Aim.canceled -= ctx => _isRightClickDown = false;

        playerActions.Gameplay.Shoot.performed -= ctx => _isLeftClickDown = true;
        playerActions.Gameplay.Shoot.canceled -= ctx => _isLeftClickDown = false;

        playerActions.Gameplay.Sprint.performed -= ctx => _isLShiftDown = true;
        playerActions.Gameplay.Sprint.canceled -= ctx => _isLShiftDown = false;

        playerActions.Gameplay.WeaponReload.performed -= ctx => _isRKeyDown = true;
        playerActions.Gameplay.WeaponReload.canceled -= ctx => _isRKeyDown = false;

        playerActions.Gameplay.ChangeGunBackward.performed -= ctx => _isQKeyDown = true;
        playerActions.Gameplay.ChangeGunBackward.canceled -= ctx => _isQKeyDown = false;

        playerActions.Gameplay.ChangeGunForward.performed -= ctx => _isEKeyDown = true;
        playerActions.Gameplay.ChangeGunForward.canceled -= ctx => _isEKeyDown = false;

        playerActions.Gameplay.Dash.performed -= ctx => _isSpaceKeyDown = true;
        playerActions.Gameplay.Dash.canceled -= ctx => _isSpaceKeyDown = false;
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