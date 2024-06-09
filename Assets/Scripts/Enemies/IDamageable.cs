using UnityEngine;

public interface IDamageable
{
    public delegate void DeathEvent(Vector3 Position);

    public delegate void TakeDamageEvent(int Damage);

    public int CurrentHealth { get; }
    public int MaxHealth { get; }

    public event TakeDamageEvent OnTakeDamage;
    public event DeathEvent OnDeath;

    public void TakeDamage(int Damage);

    Transform GetTransform();
}