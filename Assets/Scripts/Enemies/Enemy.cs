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
    public EnemyHealth Health;
    public EnemyPainResponse PainResponse;
    public Rigidbody Rigidbody;
    public int PointsForKill;


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
        
        //var points = GameObject.FindWithTag("Points").GetComponent<PlayerEnergyAndMaterialPoints>();
        //points.AddSolidMaterial(PointsForKill);
        var pointsManager = GameObject.FindWithTag("PointsManager").GetComponent<PointsManager>();
        pointsManager.RpcAddDarkEnergyPoints(PointsForKill);
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
}