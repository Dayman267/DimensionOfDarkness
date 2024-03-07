using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TilemapGeneratorInPlayer : MonoBehaviour
{
    public static Action<Vector3> OnEnteredIntoATile;

    private void OnTriggerEnter(Collider tile)
    {
        if(tile.gameObject.CompareTag("Tilemap"))
            OnEnteredIntoATile?.Invoke(tile.transform.position);
    }
}
