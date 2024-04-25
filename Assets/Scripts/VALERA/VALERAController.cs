using UnityEngine;

public class VALERAController : MonoBehaviour
{
    private Camera cam;
    
    public Transform player;
    public float heightOffset = 5f;
    public float smoothSpeed = 5f;

    private Vector3 targetPosition;
    
    void Start()
    {
        cam = Camera.main;
    }
    
    void Update()
    {
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
    }
}