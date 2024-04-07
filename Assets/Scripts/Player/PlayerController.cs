using System;
using UnityEngine;
using UnityEngine.InputSystem;


[DisallowMultipleComponent]
public class PlayerController : MonoBehaviour
{
    //[SerializeField] private Image Crosshair;
    [SerializeField] private Transform _mainCamera;
    private Camera cam;
    private Vector3 direction;
    private const float LERP_SPEED = 9;


    private Animator animator;
    private Vector3 _movementVector;

    private PlayerActions playerActions;
    private PlayerInput playerInput;

    private bool isRotating;
    private Quaternion targetRotation;

    [Header("Running")] [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 500f;
    [SerializeField] private float speedIncreaseFactor = 1.5f;
    [SerializeField] private float spendPointsWhenRunning = 0.3f;


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
    public static event Action OnShootAnimationEnable;
    public static event Action OnShootAnimationDiasble;
    public static event Action<float, float> OnSend_X_Z_Pos;
    
    public float turnSmoothTime = 0.1f;
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

    //private PlayerStamina playerStamina;
    
    private void Awake()
    {
        cam = Camera.main;

        playerInput = GetComponent<PlayerInput>();
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


        //playerStamina = GetComponent<PlayerStamina>();
        
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    static bool isRightClickDown;
    static bool isLeftClickDown;
    static bool isLShiftDown;
    static bool isRKeyDown;
    static bool isQKeyDown;
    static bool isEKeyDown;
    
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
    
    void Update()
    {
        _movementVector = CalculateMovementVector();
        direction = playerActions.Gameplay.Movement.ReadValue<Vector3>();

        bool inMovement = Mathf.Abs(direction.x) > 0 || Mathf.Abs(direction.z) > 0;

        MoveAnimEnable();

        if (isRightClickDown)
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

        if (!inMovement && !isRightClickDown && !isLeftClickDown)
        {
            ListenWASDKeyUp();
            if (isRotating)
            {
                RotateTowardsTarget();
            }
        }

        
        
        if (isDashing || isVaulting) return;

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

        Vector3 cameraR = _mainCamera.right;
        Vector3 cameraF = _mainCamera.forward;

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

        float targetMagnitude = isLShiftDown ? 1.5f : 1f;
        float lerpedMagnitude = Mathf.MoveTowards(_movementVector.magnitude, targetMagnitude, 1.1f * Time.deltaTime);
        movementVector = movementVector.normalized * lerpedMagnitude;

        Vector3 relativeVector = transform.InverseTransformDirection(movementVector);
        OnSend_X_Z_Pos?.Invoke(relativeVector.x, relativeVector.z);

        return movementVector;
    }

    private void TurnCharacterInMovementDirection()
    {
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void TurnToMousePosition()
    {
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z =
            cam.transform.position.y - transform.position.y; // Устанавливаем Z равным расстоянию от камеры до персонажа
        mousePosition = cam.ScreenToWorldPoint(mousePosition);
        // Поворот персонажа курсором мыши
        Quaternion targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
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
    
    public void Move(bool isMoving)
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f; 
        }

        if (isMoving) 
        {
            targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + Camera.main.transform.eulerAngles.y;
            if(!isLeftClickDown && !isRightClickDown) TurnCharacterInMovementDirection();

            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            float currentSpeed = isLShiftDown ? speed * speedIncreaseFactor : speed; 
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime); 
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // if (Input.GetButtonDown("Jump") && isGrounded)
        // {
        //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }
    }

    private void ShootAnimOn()
    {
        AimOn();
        OnShootAnimationEnable?.Invoke();
    }

    private void ShootAnimOff()
    {
        if (!isRightClickDown)
        {
            AimOff();
        }
        OnShootAnimationDiasble?.Invoke();
    }

    private void AimOn()
    {
        TurnToMousePosition();
        speed = 2f;
        OnAimAnimationEnable?.Invoke();
    }

    private void AimOff()
    {
        speed = 10f;
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
    }
}