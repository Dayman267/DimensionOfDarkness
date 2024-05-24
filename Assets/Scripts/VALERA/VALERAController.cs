using UnityEngine;

public class VALERAController : MonoBehaviour
{
    public Transform player;
    public float heightOffset = 5f;
    public float smoothSpeed = 5f;
    private Camera cam;

    private Vector3 targetPosition;

    private void Start()
    {
        cam = Camera.main;
    }

    private void Update()
    {
        var mousePosition = Input.mousePosition;
        var ray = cam.ScreenPointToRay(mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            var targetPoint = hit.point;
            var direction = (targetPoint - transform.position).normalized;
            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        }

        var playerPos = player.position;
        playerPos.y += heightOffset;
        targetPosition = Vector3.Lerp(transform.position, playerPos, smoothSpeed * Time.deltaTime);

        transform.position = targetPosition;
    }
}