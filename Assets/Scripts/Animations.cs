using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animations : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidbody;
    public float rotationSpeed = 10f;
    public float speed = 2f;
    
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }
    
    // Update is called once per frame
    void Update()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 directionVector = new Vector3(h, 0, v);
        if (directionVector.magnitude > Mathf.Abs(0.05f))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionVector),
                Time.deltaTime * rotationSpeed);

        _animator.SetFloat("speed", Vector3.ClampMagnitude(directionVector, 1).magnitude);
        _rigidbody.velocity = Vector3.ClampMagnitude(directionVector, 1) * speed;
    }
}
