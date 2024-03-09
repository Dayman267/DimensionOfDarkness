using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
   
    private Camera cam;
    private Vector3 direction;
    private Rigidbody rb;
    
    private Animator animator;
    private int isWalkingHash;
    private int isRunningHash;
    
    private bool isRotating;
    private Quaternion targetRotation;
    private float rotationDelay = 0.1f;
    private float rotationSmoothness = 10f;
    
    [Header("Running")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private float speedIncreaseFactor = 1.5f;
    [SerializeField] private float spendPointsWhenRunning = 0.3f;
    
    

    private bool isDashing;
    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 10f;
    [SerializeField] private float dashTime = 0.5f;
    [SerializeField] private float spendPointsWhenDashing = 10f;

    private bool isVaulting;
    [Header("Vaulting")]
    [SerializeField] private float vaultSpeed = 6f;
    [SerializeField] private float vaultTime = 0.2f;
    [SerializeField] private float stayVaultRadius = 0.6f;
    [SerializeField] private float spendPointsWhenVaulting = 20f;
    
    [SerializeField]
    private LayerMask mask;

 
    
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
        //playerStamina = GetComponent<PlayerStamina>();
    }
    
    

    void Update()
    {
        // direction.x = Input.GetAxisRaw("Horizontal");
        // direction.z = Input.GetAxisRaw("Vertical");
        //
        // direction.Normalize();
        
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);
        bool inMovement = Mathf.Abs(h) > 0 || Mathf.Abs(v) > 0;
        bool forwardPressed = Input.GetKeyUp(KeyCode.W);
        bool backwardPressed = Input.GetKeyUp(KeyCode.S);
        bool leftPressed = Input.GetKeyUp(KeyCode.A);
        bool rightPressed = Input.GetKeyUp(KeyCode.D);
        bool runPressed = Input.GetKey("left shift");

        direction = new Vector3(h, 0, v);
        direction.Normalize();

        /*Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = cam.transform.position.y - transform.position.y; // Устанавливаем Z равным расстоянию от камеры до персонажа
        mousePosition = cam.ScreenToWorldPoint(mousePosition);*/

        // Поворот персонажа курсором мыши
        /*Quaternion targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);*/
        
        Vector3 movementDirection = new Vector3(h,0,v);
        movementDirection.Normalize();
        
        
        
        transform.Translate(movementDirection * speed * Time.deltaTime, Space.World);

        if (movementDirection != Vector3.zero)
        {
            Quaternion toRotation = Quaternion.LookRotation(movementDirection,Vector3.up);

            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }
        // Баг 1 при быстром нажатии и отпускании клавиши движения модель персонажа немного смещается в этом направлении,но не успевает развернуться в нужном направлении
        if (isRotating)
        {
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            
            if (Quaternion.Angle(transform.rotation, targetRotation) < 0.1f)
            {
                isRotating = false;
            }
        }
        // Баг 1
        
        // Для плавности
        if (inMovement)
        {
            Quaternion toRotation = Quaternion.LookRotation(direction, Vector3.up);
            targetRotation = Quaternion.Slerp(transform.rotation, toRotation, Time.deltaTime * rotationSmoothness);
        }

        if (!isWalking && inMovement)
        {
            animator.SetBool(isWalkingHash,true);
        }

        if (isWalking && !inMovement)
        {
            animator.SetBool(isWalkingHash, false);
        }
        

        if (!isRunning && (inMovement && runPressed))
        {
            animator.SetBool(isRunningHash,true);
        }
        if (isRunning && (!inMovement || !runPressed))
        {
            animator.SetBool(isRunningHash,false);
        }

        
        if (!inMovement)
        {
            // Баг 1
            switch (forwardPressed, backwardPressed, leftPressed, rightPressed)
            {
                case (true,false,false,false):  RotateInDirection("forward");
                    break;
                case (false,true,false,false):  RotateInDirection("backward");
                    break;
                case (false,false,true,false):  RotateInDirection("left");
                    break;
                case (false,false,false,true):  RotateInDirection("right");
                    break;
       
                default: Debug.LogWarning("Invalid direction specified.");
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
        if(Running())
        {
            rb.velocity = direction * (speed * speedIncreaseFactor);
            //playerStamina.SpendStamina(spendPointsWhenRunning);
        }
        else
        {
            rb.velocity = direction * speed;
        }
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
