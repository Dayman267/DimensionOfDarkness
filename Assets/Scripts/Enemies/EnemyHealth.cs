using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour, IDamageable
{
    [SerializeField] private int _Health;

    public Collider Collider;
    [SerializeField] public int _MaxHealth;
    private Enemy enemy;

    private void Start()
    {
        CurrentHealth = MaxHealth;
    }

    public int CurrentHealth
    {
        get => _Health;
        private set => _Health = value < 0 ? 0 : value;
    }

    public int MaxHealth
    {
        get => _MaxHealth;
        private set => _MaxHealth = value;
    }

    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;

    public void TakeDamage(int Damage)
    {
        var damageTaken = Mathf.Clamp(Damage, 0, CurrentHealth);

        CurrentHealth -= damageTaken;

        if (damageTaken != 0) OnTakeDamage?.Invoke(damageTaken);

        if (CurrentHealth <= 0 && damageTaken != 0)
        {
            OnDeath?.Invoke(transform.position);
        }
    }

    public Transform GetTransform()
    {
        return transform;
    }
}