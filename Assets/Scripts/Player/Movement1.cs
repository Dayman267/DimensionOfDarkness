using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement1 : MonoBehaviour
{
    [SerializeField] private Transform _mainCamera;
    private Camera cam;
    private Vector3 direction;
    private Rigidbody rb;
    private const float LERP_SPEED = 9;

    private Animator animator;
    private int isAimHash;
    private int isShootingHash;
    private Vector3 _movementVector;

    private bool isRotating;
    private Quaternion targetRotation;

    [Header("Running")] [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
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

    private void Start()
    {
        // #if !UNITY_EDITOR
        //     Application.Quit();
        // #endif
        // #if UNITY_EDITOR
        //     EditorApplication.isPlaying = false;
        // #endif
        rb = GetComponent<Rigidbody>();
        cam = Camera.main;
        
        animator = GetComponent<Animator>();
        isAimHash = Animator.StringToHash("isAim");
        isShootingHash = Animator.StringToHash("isShooting");
        //playerStamina = GetComponent<PlayerStamina>();

        ResetAngularVelocity();
        transform.rotation = Quaternion.Euler(Vector3.zero);
    }

    bool isRightClickDown;
    bool isLeftClickDown;
    void Update()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        

        _movementVector = CalculateMovementVector();

        bool isAimAnimActive = animator.GetBool(isAimHash);
        bool isShootingAnimActive = animator.GetBool(isShootingHash);
        bool inMovement = Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0;
       
       

        direction = new Vector3(h, 0, v);
        direction.Normalize();

        UpdateAnimatorVariables();


        if (Input.GetKeyDown(KeyCode.Mouse1))
            isRightClickDown = true;

        if (Input.GetKeyUp(KeyCode.Mouse1))
            isRightClickDown = false;
        
        if (Input.GetKeyDown(KeyCode.Mouse0))
            isLeftClickDown = true;

        if (Input.GetKeyUp(KeyCode.Mouse0))
            isLeftClickDown = false;
        
        
        if (isLeftClickDown)
        {
            if(!isShootingAnimActive)
                    animator.SetBool(isShootingHash,true);
            if(isShootingAnimActive)
                animator.SetBool(isShootingHash,false);
        }
        else
        {
            if(isShootingAnimActive)
                animator.SetBool(isShootingHash,false);
        }

        if (isRightClickDown)
        {
            speed = 2f;
            TurnToMousePosition();
            if (!isAimAnimActive)
            {
                animator.SetBool(isAimHash, true);
            }

            
        }
        else
        {
            /*if (isLeftClickDown)
                isRightClickDown = true;*/
            speed = 10f;
            TurnCharacterInMovementDirection();
            if (isAimAnimActive)
            {
                animator.SetBool(isAimHash, false);
            }
        }

        ResetAngularVelocity();
        
        // Баг 1 при быстром нажатии и отпускании клавиши движения модель персонажа немного смещается в этом направлении,но не успевает развернуться в нужном направлении
        if (isRotating)
        {
            transform.rotation =
                Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
        // Баг 1


        if (!inMovement)
        {
            // Баг 1
            switch (Input.GetKeyUp(KeyCode.W), Input.GetKeyUp(KeyCode.S), Input.GetKeyUp(KeyCode.A),
                Input.GetKeyUp(KeyCode.D))
            {
                case (true, false, false, false):
                    RotateInDirection("forward");
                    break;
                case (false, true, false, false):
                    RotateInDirection("backward");
                    break;
                case (false, false, true, false):
                    RotateInDirection("left");
                    break;
                case (false, false, false, true):
                    RotateInDirection("right");
                    break;
            }
            // Баг 1
        }


        if (isDashing || isVaulting) return;

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
        if (Running())
        {
            rb.velocity = direction * (speed * speedIncreaseFactor);
            //playerStamina.SpendStamina(spendPointsWhenRunning);
        }
        else
        {
            rb.velocity = direction * speed;
        }
    }

    /*private void FixedUpdate()
    {
        rb.velocity = new Vector3(_movementVector.x * speed, rb.velocity.y,
            _movementVector.z * speed);
    }*/


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


    // Баг 1
    private void RotateInDirection(string direction)
    {
        Vector3 rotateDirection = Vector3.zero;

        switch (direction.ToLower())
        {
            case "forward":
                rotateDirection = Vector3.forward;
                break;
            case "backward":
                rotateDirection = Vector3.back;
                break;
            case "left":
                rotateDirection = Vector3.left;
                break;
            case "right":
                rotateDirection = Vector3.right;
                break;
            default:
                Debug.LogWarning("Invalid direction specified.");
                break;
        }

        RotateToDirection(rotateDirection);
    }
    // Баг 1


    private void UpdateAnimatorVariables()
    {
        animator.SetFloat("MovementSpeed", _movementVector.magnitude);
        //_playerAnimator.SetBool(IsAttacking, Input.GetKey(KeyCode.Mouse1));
    }

    private float maxMovementMagnitude = 1f;

    private Vector3 CalculateMovementVector()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 cameraR = _mainCamera.right;
        Vector3 cameraF = _mainCamera.forward;

        cameraR.y = 0;
        cameraF.y = 0;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            maxMovementMagnitude = 1.5f;
        }
        else
        {
            maxMovementMagnitude = 1f;
        }

        Vector3 movementVector = cameraF.normalized * v + cameraR.normalized * h;
        movementVector = Vector3.ClampMagnitude(movementVector, maxMovementMagnitude);

        // Плавное изменение магнитуды движения
        float targetMagnitude = Input.GetKey(KeyCode.LeftShift) ? 1.5f : 1f;
        float lerpedMagnitude = Mathf.MoveTowards(_movementVector.magnitude, targetMagnitude, 1.1f * Time.deltaTime);
        movementVector = movementVector.normalized * lerpedMagnitude;

        Vector3 relativeVector = transform.InverseTransformDirection(movementVector);
        animator.SetFloat("Horizontal", relativeVector.x);
        animator.SetFloat("Vertical", relativeVector.z);

        return movementVector;
    }

    private void TurnCharacterInMovementDirection()
    {
        if (rb.velocity.magnitude / speed > 0.1f)
            transform.rotation = Quaternion.Lerp(transform.rotation,
                Quaternion.LookRotation(new Vector3(rb.velocity.x, 0, rb.velocity.z)),
                LERP_SPEED * Time.deltaTime);
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

    private void ResetAngularVelocity()
    {
        rb.angularVelocity = Vector3.zero;
    }

    private bool Running() =>
        Input.GetKey(KeyCode.LeftShift) && direction != Vector3.zero; // && playerStamina.GetStaminaPoints() > 0;

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
}