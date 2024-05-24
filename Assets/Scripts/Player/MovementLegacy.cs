using UnityEngine;

public class MovementLegacy : MonoBehaviour
{
    [Header("Running")] [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float speedIncreaseFactor = 1.5f;
    [SerializeField] private float spendPointsWhenRunning = 0.3f;
    [Header("Dashing")] [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float spendPointsWhenDashing = 10f;
    [Header("Vaulting")] [SerializeField] private float vaultSpeed = 6f;
    [SerializeField] private float vaultTime = 0.2f;
    [SerializeField] private float stayVaultRadius = 0.6f;
    [SerializeField] private float spendPointsWhenVaulting = 20f;

    [SerializeField] private LayerMask mask;
    private readonly float rotationSmoothness = 10f;

    private Animator animator;
    private Camera cam;
    private Vector3 direction;
    private int isAimingHash;
    private int isAimWalk_BHash;
    private int isAimWalk_FHash;
    private int isAimWalk_LHash;
    private int isAimWalk_RHash;


    private bool isDashing;

    private bool isRotating;
    private int isRunningHash;

    private bool isVaulting;
    private int isWalkingHash;
    private Rigidbody rb;
    private Quaternion targetRotation;


    //private PlayerStamina playerStamina;

    private void Start()
    {
        // #if !UNITY_EDITOR
        //     Application.Quit();
        // #endif
        // #if UNITY_EDITOR
        //     EditorApplication.isPlaying = false;
        // #endif
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();

        animator = GetComponent<Animator>();
        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isAimingHash = Animator.StringToHash("isAiming");
        isAimWalk_FHash = Animator.StringToHash("isAimingWalkForward");
        isAimWalk_BHash = Animator.StringToHash("isAimingWalkBackward");
        isAimWalk_RHash = Animator.StringToHash("isAimingWalkRight");
        isAimWalk_LHash = Animator.StringToHash("isAimingWalkLeft");
        //playerStamina = GetComponent<PlayerStamina>();
    }


    private void Update()
    {
        // direction.x = Input.GetAxisRaw("Horizontal");
        // direction.z = Input.GetAxisRaw("Vertical");
        //
        // direction.Normalize();

        var h = Input.GetAxisRaw("Horizontal");
        var v = Input.GetAxisRaw("Vertical");

        var isWalking = animator.GetBool(isWalkingHash);
        var isRunning = animator.GetBool(isRunningHash);
        var isAiming = animator.GetBool(isAimingHash);
        var isAimWalkF = animator.GetBool(isAimWalk_FHash);
        var isAimWalkB = animator.GetBool(isAimWalk_BHash);
        var isAimWalkR = animator.GetBool(isAimWalk_RHash);
        var isAimWalkL = animator.GetBool(isAimWalk_LHash);
        var inMovement = Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0;
        var forwardPressed = Input.GetKeyUp(KeyCode.W);
        var backwardPressed = Input.GetKeyUp(KeyCode.S);
        var leftPressed = Input.GetKeyUp(KeyCode.A);
        var rightPressed = Input.GetKeyUp(KeyCode.D);
        var runPressed = Input.GetKey("left shift");
        var rightClickPressed = Input.GetKey(KeyCode.Mouse1);

        direction = new Vector3(h, 0, v);
        direction.Normalize();


        if (rightClickPressed)
        {
            speed = 2f;
            var mousePosition = Input.mousePosition;
            mousePosition.z =
                cam.transform.position.y -
                transform.position.y; // Устанавливаем Z равным расстоянию от камеры до персонажа
            mousePosition = cam.ScreenToWorldPoint(mousePosition);
            // Поворот персонажа курсором мыши
            var targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
            transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);

            OffRun(isWalking, inMovement);
            OffSprint(isRunning, inMovement, runPressed);

            if (!isAiming) animator.SetBool(isAimingHash, true);

            if (!isAimWalkF && isAiming && Input.GetKey(KeyCode.W)) animator.SetBool(isAimWalk_FHash, true);

            if (!isAimWalkB && isAiming && Input.GetKey(KeyCode.S)) animator.SetBool(isAimWalk_BHash, true);

            if (!isAimWalkR && isAiming && Input.GetKey(KeyCode.D)) animator.SetBool(isAimWalk_RHash, true);

            if (!isAimWalkL && isAiming && Input.GetKey(KeyCode.A)) animator.SetBool(isAimWalk_LHash, true);

            if (isAimWalkF && !Input.GetKey(KeyCode.W)) animator.SetBool(isAimWalk_FHash, false);

            if (isAimWalkB && !Input.GetKey(KeyCode.S)) animator.SetBool(isAimWalk_BHash, false);

            if (isAimWalkR && !Input.GetKey(KeyCode.D)) animator.SetBool(isAimWalk_RHash, false);

            if (isAimWalkL && !Input.GetKey(KeyCode.A)) animator.SetBool(isAimWalk_LHash, false);
        }
        else
        {
            speed = 5f;

            var movementDirection = new Vector3(h, 0, v);
            movementDirection.Normalize();

            transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);

            if (movementDirection != Vector3.zero)
            {
                var toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);

                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
            }

            // Баг 1 при быстром нажатии и отпускании клавиши движения модель персонажа немного смещается в этом направлении,но не успевает развернуться в нужном направлении
            if (isRotating)
            {
                transform.rotation =
                    Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

                if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f) isRotating = false;
            }
            // Баг 1

            // Для плавности
            if (inMovement)
            {
                var toRotation = Quaternion.LookRotation(direction, Vector3.up);
                targetRotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * rotationSmoothness);
            }

            if (isAiming) animator.SetBool(isAimingHash, false);

            if (!isWalking && inMovement) animator.SetBool(isWalkingHash, true);

            OffRun(isWalking, inMovement);

            if (!isRunning && inMovement && runPressed) animator.SetBool(isRunningHash, true);

            OffSprint(isRunning, inMovement, runPressed);


            if (!inMovement)
                // Баг 1
                switch (forwardPressed, backwardPressed, leftPressed, rightPressed)
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


        //transform.rotation.SetLookRotation(mousePosition);

        // Vector3 difference = cam.ScreenToWorldPoint(Input.mousePosition) - transform.position;
        // Debug.Log(difference);
        // float rotate = Mathf.Atan2(difference.y, difference.x) * Mathf.Rad2Deg;
        // transform.rotation = Quaternion.Euler(rotate-90, rotate-90, rotate-90);
        //transform.eulerAngles = new Vector3(0f, rotate-90);

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
            rb.velocity = direction * (speed * speedIncreaseFactor);
        //playerStamina.SpendStamina(spendPointsWhenRunning);
        else
            rb.velocity = direction * speed;
    }

    private void OffRun(bool animState, bool movementState)
    {
        if (animState && !movementState) animator.SetBool(isWalkingHash, false);
    }

    private void OffSprint(bool animState, bool movementState, bool runButtState)
    {
        if (animState && (!movementState || !runButtState)) animator.SetBool(isRunningHash, false);
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


    // Баг 1
    private void RotateInDirection(string direction)
    {
        var rotateDirection = Vector3.zero;

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

    private bool Running()
    {
        return Input.GetKey(KeyCode.LeftShift) && direction != Vector3.zero;
        // && playerStamina.GetStaminaPoints() > 0;
    }

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