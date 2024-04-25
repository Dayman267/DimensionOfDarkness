using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletEffect : MonoBehaviour
{
   [SerializeField] private float destroyDelay = 3f;

    private void Start()
    {
        Invoke("DestroyObject", destroyDelay);
    }

    private void DestroyObject()
    {
        Destroy(gameObject);
    }
}
