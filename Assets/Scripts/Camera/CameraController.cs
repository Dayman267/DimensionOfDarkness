using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float offsetX;
    public float offsetY = 5f;
    public float offsetZ = -5f;
    public float rotation = 60f;

    private void Update()
    {
        var pos = player.transform.position;
        transform.position = new Vector3(pos.x + offsetX, pos.y + offsetY, pos.z + offsetZ);
        transform.eulerAngles = new Vector3(rotation, 0f);
    }
}