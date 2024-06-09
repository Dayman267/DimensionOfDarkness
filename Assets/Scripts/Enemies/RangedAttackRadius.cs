using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class RangedAttackRadius : AttackRadius
{
    public EnemyBullet BulletPrefab;
    public Vector3 BulletSpawnOffset = new(0, 1, 0);
    public LayerMask Mask;

    [SerializeField] private float SpherecastRadius = 0.1f;

    private EnemyBullet bullet;
    private ObjectPool.ObjectPool BulletPool;
    private RaycastHit Hit;
    private IDamageable targetDamageable;


    public void CreateBulletPool()
    {
        if (BulletPool == null)
        {
            BulletPool = ObjectPool.ObjectPool.CreateInstance(BulletPrefab,
                Mathf.CeilToInt(1 / AttackDelay * BulletPrefab.AutoDestroyTime));
        }
    }

    protected override void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);

        if (AttackCoroutine == null) Agent.enabled = true;
    }

    protected override IEnumerator Attack()
    {
        var Wait = new WaitForSeconds(AttackDelay);


        while (Damageables.Count > 0)
        {
            for (var i = 0; i < Damageables.Count; i++)
                if (HasLineOfSightTo(Damageables[i].GetTransform()))
                {
                    targetDamageable = Damageables[i];
                    OnAttack?.Invoke(Damageables[i]);
                    Agent.enabled = false;
                    break;
                }

            if (targetDamageable != null)
            {
                var poolableObject = BulletPool?.GetObject();
                if (poolableObject != null)
                {
                    bullet = poolableObject.GetComponent<EnemyBullet>();

                    bullet.Damage = Damage;
                    bullet.transform.position = transform.position + BulletSpawnOffset;
                    bullet.transform.rotation = Agent.transform.rotation;
                    bullet.Rigidbody.AddForce(Agent.transform.forward * BulletPrefab.MoveSpeed,
                        ForceMode.VelocityChange);
                }
            }
            else
            {
                Agent.enabled = true; // no target in line of sight, keep trying to get closer
            }

            yield return Wait;

            if (targetDamageable == null || !HasLineOfSightTo(targetDamageable.GetTransform())) Agent.enabled = true;

            Damageables.RemoveAll(DisabledDamageables);
            yield return Wait;
        }

        Agent.enabled = true;
        AttackCoroutine = null;
    }

    private bool HasLineOfSightTo(Transform Target)
    {
        if (Physics.SphereCast(transform.position + BulletSpawnOffset, SpherecastRadius,
                (Target.position + BulletSpawnOffset - (transform.position + BulletSpawnOffset)).normalized, out Hit,
                Collider.radius, Mask))
        {
            IDamageable damageable;
            if (Hit.collider.TryGetComponent(out damageable)) return damageable.GetTransform() == Target;
        }
        return false;
    }
}