using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : ObjectPool.PoolableObject
{
    private const string ATTACK_TRIGGER = "Attack";
    public AttackRadius AttackRadius;
    public Animator Animator;
    public EnemyMovement Movement;
    public NavMeshAgent Agent;
    public EnemyScriptableObject EnemyScriptableObject;
    public EnemyHealth Health;
    public EnemyPainResponse PainResponse;
    public Rigidbody Rigidbody;

    private Coroutine LookCoroutine;

    private void Die(Vector3 Position)
    {
        Movement.StopMoving();
        PainResponse.HandleDeath();
        Health.OnTakeDamage -= PainResponse.HandlePain;
        Health.OnDeath -= Die;
        AttackRadius.OnAttack -= OnAttack;
        AttackRadius.StopAttack();
        AttackRadius.Collider.enabled = false;
        Health.Collider.isTrigger = true;
        Rigidbody.constraints = RigidbodyConstraints.FreezePosition;
        Invoke(nameof(Trash), 5f);
        
        Agent.enabled = false;
    }

    private void Trash()
    {
        Rigidbody.constraints = RigidbodyConstraints.None;
        Destroy(gameObject, 10);
    }
    private void Awake()
    {
        Health.OnTakeDamage += PainResponse.HandlePain;
        Health.OnDeath += Die;
        AttackRadius.OnAttack += OnAttack;
    }

    public void OnEnable()
    {
        SetupAgentFromConfiguration();
        
    }

    public override void OnDisable()
    {
        base.OnDisable();

    }

    private void OnAttack(IDamageable Target)
    {
        Animator.SetTrigger(ATTACK_TRIGGER);

        if (LookCoroutine != null) StopCoroutine(LookCoroutine);

        LookCoroutine = StartCoroutine(LookAt(Target.GetTransform()));
    }

    private IEnumerator LookAt(Transform Target)
    {
        var lookRotation = Quaternion.LookRotation(Target.position - transform.position);
        float time = 0;

        while (time < 1)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, time);

            time += Time.deltaTime * 2;
            yield return null;
        }

        transform.rotation = lookRotation;
    }

    public virtual void SetupAgentFromConfiguration()
    {
        Agent.acceleration = EnemyScriptableObject.Acceleration;
        Agent.angularSpeed = EnemyScriptableObject.AngularSpeed;
        Agent.areaMask = EnemyScriptableObject.AreaMask;
        Agent.avoidancePriority = EnemyScriptableObject.AvoidancePriority;
        Agent.baseOffset = EnemyScriptableObject.BaseOffset;
        Agent.height = EnemyScriptableObject.Height;
        Agent.obstacleAvoidanceType = EnemyScriptableObject.ObstacleAvoidanceType;
        Agent.radius = EnemyScriptableObject.Radius;
        Agent.speed = EnemyScriptableObject.Speed;
        Agent.stoppingDistance = EnemyScriptableObject.StoppingDistance;

        Movement.UpdateRate = EnemyScriptableObject.AIUpdateInterval;

        Health._MaxHealth = EnemyScriptableObject.Health;
        AttackRadius.Collider.radius = EnemyScriptableObject.AttackRadius;
        AttackRadius.AttackDelay = EnemyScriptableObject.AttackDelay;
        AttackRadius.Damage = EnemyScriptableObject.Damage;
    }
}