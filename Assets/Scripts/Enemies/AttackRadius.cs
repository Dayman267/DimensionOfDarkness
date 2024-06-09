using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class AttackRadius : MonoBehaviour
{
    public delegate void AttackEvent(IDamageable Target);

    public SphereCollider Collider;
    public int Damage = 10;
    public float AttackDelay = 0.5f;
    protected Coroutine AttackCoroutine;
    protected List<IDamageable> Damageables = new();
    public AttackEvent OnAttack;

    protected virtual void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        Damageables.Add(damageable);

        StartAttack();
    }

    public void StartAttack()
    {
        AttackCoroutine ??= StartCoroutine(Attack());
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        if (!other.TryGetComponent<IDamageable>(out var damageable)) return;
        Damageables.Remove(damageable);
        if (Damageables.Count != 0) return;
        StopAttack();
    }

    public void StopAttack()
    {
        if (AttackCoroutine != null)
            StopCoroutine(AttackCoroutine);
        AttackCoroutine = null;
    }

    protected virtual IEnumerator Attack()
    {
        var Wait = new WaitForSeconds(AttackDelay);

        

        IDamageable closestDamageable = null;
        var closestDistance = float.MaxValue;
    
        while (Damageables.Count > 0)
        {
            for (var i = 0; i < Damageables.Count; i++)
            {
                var damageableTransform = Damageables[i].GetTransform();
                var distance = Vector3.Distance(transform.position, damageableTransform.position);

                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestDamageable = Damageables[i];
                }
            }

            if (closestDamageable != null)
            {
                OnAttack?.Invoke(closestDamageable);
                closestDamageable.TakeDamage(Damage);
            }

            closestDamageable = null;
            closestDistance = float.MaxValue;

            yield return Wait;

            Damageables.RemoveAll(DisabledDamageables);
        }

        AttackCoroutine = null;
    }

    protected bool DisabledDamageables(IDamageable Damageable)
    {
        return Damageable != null && !Damageable.GetTransform().gameObject.activeSelf;
    }
}