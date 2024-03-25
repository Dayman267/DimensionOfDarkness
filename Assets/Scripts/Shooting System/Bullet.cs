using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Bullet : MonoBehaviour
{
    private Rigidbody rigidbody;

    [field: SerializeField]
    public Vector3 SpawnLocation
    {
        get;
        private set;
    }

    [SerializeField] private float DeleyedDisableTime = 2f;

    public delegate void CollisionEvent(Bullet Bullet, Collision Collision);
    public event CollisionEvent OnCollision;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
    }

    public void Spawn(Vector3 SpawnForce)
    {
        SpawnLocation = transform.position;
        transform.forward = SpawnForce.normalized;
        rigidbody.AddForce(SpawnForce);
        StartCoroutine(DeleyedDisable(DeleyedDisableTime));
    }

    private IEnumerator DeleyedDisable(float Time)
    {
        yield return new WaitForSeconds(Time);
        OnCollisionEnter(null);
    }

    private void OnCollisionEnter(Collision Collision)
    {
        OnCollision?.Invoke(this, Collision);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        OnCollision = null;

    }
}
