using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    private GameObject player;
    void Start()
    {
        player = GameObject.FindWithTag("Player");
    }

    void Update()
    {
        transform.LookAt(player.transform);
    }
}
