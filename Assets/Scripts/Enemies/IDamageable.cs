using System;
using UnityEngine;

public interface IDamageable
{
   public int CurrentHealth { get; }
   public int MaxHealth { get; }

   /*public delegate void TakeDamageEvent(int Damage);

   public event TakeDamageEvent OnTakeDamage;*/

   public delegate void DeathEvent(Vector3 Position);
   public event DeathEvent OnDeath;

   public void TakeDamage(int Damage);
   
   public event Action<int> OnTakeDamage;
}
