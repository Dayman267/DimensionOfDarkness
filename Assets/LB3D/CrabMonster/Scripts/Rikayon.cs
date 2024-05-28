using UnityEngine;

public class Rikayon : MonoBehaviour
{
    public Animator animator;

    // Use this for initialization
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)) animator.SetTrigger("Attack_1");
    }
}