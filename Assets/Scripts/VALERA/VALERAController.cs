using UnityEngine;

public class VALERAController : MonoBehaviour, IPausable
{
    private Camera cam;

    public Transform player;
    public float heightOffset = 5f;
    public float smoothSpeed = 5f;

    public float canvasSmoothSpeed = 200f;
    private Transform canvas;

    private Vector3 targetPosition;

    private float originalRotationX;

    private bool isRotating = false;

    private bool isPaused = false;

    private void OnEnable()
    {
        PauseGame.OnGamePaused += OnPause;
        PauseGame.OnGameResumed += OnResume;
    }

    private void OnDisable()
    {
        PauseGame.OnGamePaused -= OnPause;
        PauseGame.OnGameResumed -= OnResume;
    }
    
    void Start()
    {
        cam = Camera.main;
        canvas = GetComponentInChildren<Canvas>().transform;
        originalRotationX = transform.rotation.x;
    }

    void Update()
    {
        if (isPaused) return;
        
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = cam.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            Vector3 targetPoint = hit.point;
            Vector3 direction = (targetPoint - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        Vector3 playerPos = player.position;
        playerPos.y += heightOffset;
        targetPosition = Vector3.Lerp(transform.position, playerPos, smoothSpeed * Time.deltaTime);

        transform.position = targetPosition;


        
        Vector2 upperCenter = new Vector2(Screen.width / 2, Screen.height / 2);

        float targetRotationX = 0;
        float targetRotationY = 0;

        // Если курсор находится в верхней части экрана
        if (mousePosition.y > upperCenter.y)
        {
            if (mousePosition.x < Screen.width / 3)
            {
                // Верхний левый угол
                targetRotationX = 50;
                targetRotationY = 55;
            }
            else if (mousePosition.x > 2 * Screen.width / 3)
            {
                // Верхний правый угол
                targetRotationX = 50;
                targetRotationY = 300;
            }
            else
            {
                // Центральная верхняя часть экрана
                targetRotationX = 0;
                targetRotationY = 0;
            }
        }
        else if (mousePosition.y < upperCenter.y)
        {
            if (mousePosition.x < Screen.width / 3)
            {
                // Нижний левый угол
                targetRotationX = 50;
                targetRotationY = 65;
            }
            else if (mousePosition.x > 2 * Screen.width / 3)
            {
                // Нижний правый угол
                targetRotationX = 50;
                targetRotationY = 290;
            }
            else
            {
                // Центральная Нижний часть экрана
                targetRotationX = 90;
                targetRotationY = 0;
            }
        }

        float currentRotationX =
            Mathf.Lerp(canvas.localEulerAngles.x, targetRotationX, Time.deltaTime * canvasSmoothSpeed);
        float currentRotationY =
            Mathf.Lerp(canvas.localEulerAngles.y, targetRotationY, Time.deltaTime * canvasSmoothSpeed);
        canvas.localEulerAngles = new Vector3(currentRotationX, currentRotationY , canvas.localEulerAngles.z);
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