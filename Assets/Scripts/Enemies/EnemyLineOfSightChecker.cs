using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class EnemyLineOfSightChecker : MonoBehaviour
{
    public SphereCollider Collider;
    public float FieldOfView = 90f;
    public LayerMask LineOfSightLayers;

    public delegate void GainSightEvent(PlayerController player);

    public GainSightEvent OnGainSight;

    public delegate void LoseSightEvent(PlayerController player);

    public LoseSightEvent OnLoseSight;
    private Coroutine CheckForLineOfSightCoroutine;
    private void Awake()
    {
        Collider = GetComponent<SphereCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent(out player))
        {
            if (!CheckLineOfSight(player))
            {
                CheckForLineOfSightCoroutine = StartCoroutine(CheckForLineOfSight(player));
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PlayerController player;
        if (other.TryGetComponent(out player))
        {
            OnLoseSight?.Invoke(player);
            if (CheckForLineOfSightCoroutine != null)
            {
                StopCoroutine(CheckForLineOfSightCoroutine);
            }
        }
    }

    private bool CheckLineOfSight(PlayerController player)
    {
        Vector3 Direction = (player.transform.position - transform.position).normalized;
        if (Vector3.Dot(transform.forward, Direction) >= Mathf.Cos(FieldOfView))
        {
            RaycastHit Hit;

            if (Physics.Raycast(transform.position, Direction, out Hit, Collider.radius, LineOfSightLayers))
            {
                if (Hit.transform.GetComponent<PlayerController>() != null)
                {
                    OnGainSight?.Invoke(player);
                    return true;
                }
            }
        }
        return false;
    }

    private IEnumerator CheckForLineOfSight(PlayerController player)
    {
        WaitForSeconds Wait = new WaitForSeconds(0.1f);

        while (!CheckLineOfSight(player))
        {
            yield return Wait;
        }
    }
}
