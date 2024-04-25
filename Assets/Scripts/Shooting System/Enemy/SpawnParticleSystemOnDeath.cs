using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(IDamageable))]
public class SpawnParticleSystemOnDeath : MonoBehaviour
{
    [SerializeField] private ParticleSystem DeathSystem;
    public IDamageable Damageable;

    private void Awake()
    {
        Damageable = GetComponent<IDamageable>();
    }

    private void OnEnable()
    {
        Damageable.OnDeath += DamageableDeathHandler;
    }

    private void DamageableDeathHandler(Vector3 Position)
    {
        Instantiate(DeathSystem, Position, Quaternion.identity);
    }
}
