using System;
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
    private float originSmoothSpeed;

    private void Awake()
    {
        player = GameObject.FindWithTag("Player").transform;
        cam = Camera.main;
        originSmoothSpeed = smoothSpeed;
    }

    
    
    /*private void Update()
    {
        Vector3 playerPos = player.position;
        transform.LookAt(cam.transform);
    
        // Используйте Mathf.Sign() для определения знака PlayerController.direction.x
        playerPos.x += -Mathf.Sign(PlayerController.direction.x) * Mathf.Abs(posXOffset);
        playerPos.y += posYOffset;
        playerPos.z += posZOffset;
    
        targetPosition = Vector3.Lerp(transform.position, playerPos, smoothSpeed * Time.deltaTime);
        

        /*if (transform.position != targetPosition && smoothSpeed <= 100f)
        {
            smoothSpeed += Time.deltaTime * speedAccelerationFactor;
        }
        else if(!PlayerController.inMovement)
        {
            smoothSpeed = originSmoothSpeed;
        }#1#
    
        transform.position = targetPosition;
    }*/

    private void Update()
    {
        transform.LookAt(cam.transform);
        
        Vector3 playerPos = player.position;
        /*playerPos.x += posXOffset;
        playerPos.y += posYOffset;
        playerPos.z += posZOffset;*/
    }
}
