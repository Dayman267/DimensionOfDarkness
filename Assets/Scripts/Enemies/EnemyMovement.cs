using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private const string IsWalking = "IsWalking";
    public Transform Player;

    [SerializeField] private Animator Animator;

    public float UpdateRate = 0.1f;
    private NavMeshAgent Agent;

    private Coroutine FollowCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        if (Player != null && Agent.enabled)
        {
            StartCoroutine(FollowTarget());
        }
        else
        {
            Debug.LogError("Player Transform is not assigned or NavMeshAgent is not enabled in EnemyMovement.");
        }
    }

    public void Update()
    {
        Animator.SetBool(IsWalking, Agent.velocity.magnitude > 0.01f);
    }

    public void StartChasing()
    {
        if (FollowCoroutine == null)
            FollowCoroutine = StartCoroutine(FollowTarget());
        else
            Debug.LogWarning(
                "Called StartChasing on Enemy that is already chasing! This is likely a bug in some calling class!");
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        Agent.isStopped = true;
        Agent.enabled = false;
    }

    private IEnumerator FollowTarget()
    {
        var Wait = new WaitForSeconds(UpdateRate);

        while (enabled)
        {
            if (Player != null)
            {
                Agent.SetDestination(Player.position);
            }

            yield return Wait;
        }
    }
}