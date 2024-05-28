using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float rotationSpeed = 10f;
    public float speed = 2f;
    public int numberOfAttacks = 2; // Количество анимаций атаки
    private Animator _animator;
    private Rigidbody _rigidbody;

    private bool isAttacking;

    // Start is called before the first frame update
    private void Start()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");
        var canAttack = !isAttacking && Input.GetMouseButtonDown(0);

        var directionVector = new Vector3(-v, 0, h);
        if (directionVector.magnitude > Mathf.Abs(0.05f))
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(directionVector),
                Time.deltaTime * rotationSpeed);

        _animator.SetFloat("speed", Vector3.ClampMagnitude(directionVector, 1).magnitude);
        _rigidbody.velocity = Vector3.ClampMagnitude(directionVector, 1) * speed;

        if (canAttack)
        {
            isAttacking = true;
            var randomAttackIndex = Random.Range(0, numberOfAttacks); // Генерация случайного числа для выбора атаки
            _animator.SetInteger("attackIndex", randomAttackIndex); // Установка параметра аниматора для выбора атаки
            _animator.SetTrigger("attackTrigger");
        }
    }

    public void OnAttackAnimationEnd()
    {
        isAttacking = false;
    }
}