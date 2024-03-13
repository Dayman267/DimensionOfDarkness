using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    private Animator _animator;
    private Rigidbody _rigidbody;
    private bool isAttacking = false;
    
    public float rotationSpeed = 10f;
    public float speed = 2f;
    public int numberOfAttacks = 2; // Количество анимаций атаки
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
        bool canAttack = !isAttacking && Input.GetMouseButtonDown(0);
        
        Vector3 directionVector = new Vector3(-v, 0, h);
        if(directionVector.magnitude > Mathf.Abs(0.05f))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionVector),
                Time.deltaTime * rotationSpeed);
        
        _animator.SetFloat("speed", Vector3.ClampMagnitude(directionVector, 1).magnitude);
        _rigidbody.velocity =  Vector3.ClampMagnitude(directionVector,1) * speed;

        if (canAttack)
        {
            isAttacking = true;
            int randomAttackIndex = Random.Range(0, numberOfAttacks); // Генерация случайного числа для выбора атаки
            _animator.SetInteger("attackIndex", randomAttackIndex); // Установка параметра аниматора для выбора атаки
            _animator.SetTrigger("attackTrigger");
        }
    }
    
    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
}

