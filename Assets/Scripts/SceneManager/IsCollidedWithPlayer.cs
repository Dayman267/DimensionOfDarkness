using System;
using UnityEngine;

public class IsCollidedWithPlayer : MonoBehaviour
{
    public static Action onTriggeredWithPlayer;
    public static Action onPlayerTriggerExit;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) onTriggeredWithPlayer?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player")) onPlayerTriggerExit?.Invoke();
    }
}
