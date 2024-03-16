using UnityEngine;

public class Movement : MonoBehaviour
{
    private Camera cam;
    private Vector3 direction;
    private Rigidbody rb;
    
    [Header("Running")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float speedIncreaseFactor = 1.5f;
    [SerializeField] private float spendPointsWhenRunning = 0.3f;
    
    // [SerializeField] private float maxSlopeAngle = 45f; // Максимальный угол наклона поверхности
    // [SerializeField] private float maxSlopeDistance = 0.2f; // Максимальное расстояние для определения поверхности
    // [SerializeField] private LayerMask terrainLayer; // Слой, на котором находится террейн

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

        direction = new Vector3(h, 0, v);
        direction.Normalize();
        
        // float terrainSlope = CalculateTerrainSlope();
        // if (Mathf.Abs(terrainSlope) > maxSlopeAngle)
        // {
        //     direction = Vector3.zero;
        // }
        // float speedMultiplier = 1f - Mathf.Abs(terrainSlope) / maxSlopeAngle;
        // float finalSpeed = speed * speedMultiplier;
        
        Vector3 mousePosition = Input.mousePosition;
        mousePosition.z = cam.transform.position.y - transform.position.y; // Устанавливаем Z равным расстоянию от камеры до персонажа
        mousePosition = cam.ScreenToWorldPoint(mousePosition);

        // Поворот персонажа курсором мыши
        Quaternion targetRotation = Quaternion.LookRotation(mousePosition - transform.position);
        transform.rotation = Quaternion.Euler(0f, targetRotation.eulerAngles.y, 0f);
        
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
    
    // float CalculateTerrainSlope()
    // {
    //     RaycastHit hit;
    //     // Пускаем луч вниз, чтобы определить угол наклона террейна
    //     if (Physics.Raycast(transform.position, Vector3.down, out hit, maxSlopeDistance, terrainLayer))
    //     {
    //         float slopeAngle = Vector3.Angle(hit.normal, Vector3.up);
    //         return slopeAngle;
    //     }
    //     return 0f;
    // }
}
