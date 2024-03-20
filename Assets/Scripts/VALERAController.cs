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
        Vector3 mousePosition = cam.ScreenToWorldPoint(new Vector3(
            Input.mousePosition.x, 
            Input.mousePosition.y, 
            Camera.main.transform.position.y));
        Vector3 direction = (mousePosition - transform.position).normalized;
        transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
    }

    private void FixedUpdate()
    {
        Vector3 playerPos = player.position;
        playerPos.y += heightOffset;
        targetPosition = Vector3.Lerp(transform.position, playerPos, smoothSpeed * Time.fixedDeltaTime);

        transform.position = targetPosition;
    }
}