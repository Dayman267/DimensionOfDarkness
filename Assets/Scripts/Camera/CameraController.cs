using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;
    public float offsetX = 0f;
    public float offsetY = 5f;
    public float offsetZ = -5f;
    public float rotation = 60f;

    void Update()
    {
        Vector3 pos = player.transform.position;
        transform.position = new Vector3(pos.x + offsetX, pos.y + offsetY, pos.z + offsetZ);
        transform.eulerAngles = new Vector3(rotation, 0f);
    }
}
