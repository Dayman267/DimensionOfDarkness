using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private Object weaponVFX;
    private Camera cam;
    private Vector3 direction;
    
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

    [SerializeField] private LayerMask mask;
    //private PlayerStamina playerStamina;

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

    private void Awake()
    {
        cam = Camera.main;

        playerInput = GetComponent<PlayerInput>();
        playerActions = new PlayerActions();
        playerActions.Gameplay.Enable();
        //playerActions.Gameplay.Aim.performed += Aim;
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

        controller = GetComponent<CharacterController>();
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    bool isRightClickDown;
    bool isLeftClickDown;
    bool isLShiftDown;
    bool isShootingWhileRun;

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
            ShootOn(inMovement);
        }
        else
        {
            ShootOff();
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
        // Баг 1    
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
        // Баг 1
    }

    // Баг 1
    private void RotateToDirection(Vector3 direction)
    {
        if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            isRotating = true;
        }
    }
    // Баг 1

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

    public void FootStep()
    {
        // Воспроизведение звука шагов
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

    // private bool IsSprinting() =>
    //     isLShiftDown && direction != Vector3.zero; // && playerStamina.GetStaminaPoints() > 0;

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
            float currentSpeed = isLShiftDown ? speed*speedIncreaseFactor : speed; 
            controller.Move(moveDir.normalized * currentSpeed * Time.deltaTime); 
        }
        
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // if (Input.GetButtonDown("Jump") && isGrounded)
        // {
        //     velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        // }
    }

    private void TurnCharacterInMovementDirection()
    {
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
        transform.rotation = Quaternion.Euler(0f, angle, 0f);
    }

    private void ShootOn(bool isMove)
    {
        if (isMove && !isRightClickDown)
        {
            isShootingWhileRun = true;
        }

        if (isShootingWhileRun)
        {
            AimOn();
        }

        OnShootAnimationEnable?.Invoke();
        weaponVFX.GameObject().SetActive(true);
    }

    private void ShootOff()
    {
        if (isShootingWhileRun && !isRightClickDown)
        {
            AimOff();
            isShootingWhileRun = false;
        }


        OnShootAnimationDiasble?.Invoke();
        weaponVFX.GameObject().SetActive(false);
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
    }

    private void OnDisable()
    {
        playerActions.Gameplay.Disable();
        playerActions.Gameplay.Aim.performed -= ctx => isRightClickDown = true;
        playerActions.Gameplay.Aim.canceled -= ctx => isRightClickDown = false;

        playerActions.Gameplay.Shoot.performed -= ctx => isLeftClickDown = true;
        playerActions.Gameplay.Shoot.canceled -= ctx => isLeftClickDown = false;

        playerActions.Gameplay.Sprint.performed += ctx => isLShiftDown = true;
        playerActions.Gameplay.Sprint.canceled += ctx => isLShiftDown = false;
    }
}