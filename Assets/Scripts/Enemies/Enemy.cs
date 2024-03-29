using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyHealth Health;

    private void OnDisable()
    {
        Health.OnDeath -= Die;
    }

    private void OnEnable()
    {
        Health.OnDeath += Die;
    }

    private void Start()
    {
        //Health.OnTakeDamage += PainResponse.Handler;
        
    }

    private void Die(Vector3 position)
    {
        Destroy(gameObject);
    }
}
