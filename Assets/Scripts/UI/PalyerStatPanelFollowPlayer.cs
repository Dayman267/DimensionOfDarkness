using UnityEngine;
using UnityEngine.InputSystem;

public class PalyerStatPanelFollowPlayer : MonoBehaviour
{
    private Camera cam;
    private Transform player;
    private Vector3 targetPosition;
    public float smoothSpeed = 5f;
    public float posXOffset = 5f;
    public float posYOffset = 5f;
    public float posZOffset = 5f;
    public float speedAccelerationFactor = 1f;
    private float originsmothSpeed;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        cam = Camera.main;
        originsmothSpeed = smoothSpeed;
    }

    
    
    private void Update()
    {
        Vector3 playerPos = player.position;
        transform.LookAt(cam.transform);
    
        // Используйте Mathf.Sign() для определения знака PlayerController.direction.x
        playerPos.x += -Mathf.Sign(PlayerController.direction.x) * Mathf.Abs(posXOffset);
        playerPos.y += posYOffset;
        playerPos.z += posZOffset;
    
        targetPosition = Vector3.Lerp(transform.position, playerPos, smoothSpeed * Time.deltaTime);

        if (PlayerController.direction.x < 0)
        {
            Debug.Log("isGoing Left");
        }
        else
        {
            Debug.Log("isGoing Right");
        }

        if (transform.position != targetPosition && smoothSpeed <= 100f)
        {
            smoothSpeed += Time.deltaTime * speedAccelerationFactor;
        }
        else if(!PlayerController.inMovement)
        {
            smoothSpeed = originsmothSpeed;
        }
    
        transform.position = targetPosition;
    }
}
