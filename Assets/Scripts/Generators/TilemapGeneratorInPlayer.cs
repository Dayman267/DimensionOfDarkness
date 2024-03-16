using System;
using UnityEngine;

public class TilemapGeneratorInPlayer : MonoBehaviour
{
    public static Action<Vector3> OnEnteredIntoATile;

    private void OnTriggerEnter(Collider tile)
    {
        if(tile.gameObject.CompareTag("Terrain"))
            OnEnteredIntoATile?.Invoke(tile.transform.position);
    }
}
