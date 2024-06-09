using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;
using Random = UnityEngine.Random;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovement : MonoBehaviour
{
    private const string IsWalking = "IsWalking";
    public Transform Player;
    public EnemyLineOfSightChecker LineOfSightChecker;
    [SerializeField] private Animator Animator;
    public float UpdateRate = 0.1f;
    private NavMeshAgent Agent;

    public EnemyState DefaultState;
    private EnemyState _state;
    public EnemyState State
    {
        get { return _state; }
        set
        {
            OnStateChange?.Invoke(_state, value);
            _state = value;
        }
    }

    public delegate void StateChangeEvent(EnemyState oldState, EnemyState newState);

    public StateChangeEvent OnStateChange;
    public float IdleLocationRadius = 4f;
    public float IdleMovespeedMultiplier = 0.5f;
    
    private Coroutine FollowCoroutine;

    private void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();

        LineOfSightChecker.OnGainSight += HandleGainSight;
        LineOfSightChecker.OnLoseSight += HandleLoseSight;
        
        OnStateChange += HandleStateChange;
    }

    private void HandleGainSight(PlayerController player)
    {
        State = EnemyState.Chase;
    }

    private void HandleLoseSight(PlayerController player)
    {
        State = DefaultState;
    }

    private void OnDisable()
    {
        _state = DefaultState;
    }

    private void Start()
    {
        if (Player != null && Agent.enabled)
        {
            State = DefaultState; // Ensure the initial state is set
            if (State != EnemyState.Idle)
            {
                FollowCoroutine = StartCoroutine(FollowTarget());
            }
            /*else
            {
                FollowCoroutine = StartCoroutine(DoIdleMotion());
            }*/
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

    public void Spawn()
    {
        OnStateChange(EnemyState.Spawn, DefaultState);
    }

    public void StopMoving()
    {
        StopAllCoroutines();
        if (!Agent.enabled) return;
        Agent.isStopped = true;
        Agent.enabled = false;
    }
    
    private void HandleStateChange(EnemyState oldState, EnemyState newState)
    {
        if (oldState != newState)
        {
            if (FollowCoroutine != null)
            {
                StopCoroutine(FollowCoroutine);
            }

            if (oldState == EnemyState.Idle)
            {
                Agent.speed /= IdleMovespeedMultiplier;
            }
            
            switch (newState)
            {
                case EnemyState.Idle:
                    FollowCoroutine = StartCoroutine(DoIdleMotion());
                    break;
                case EnemyState.Chase:
                    FollowCoroutine = StartCoroutine(FollowTarget());
                    break;
            }
        }
    }

    private IEnumerator FollowTarget()
    {
        var Wait = new WaitForSeconds(UpdateRate);

        while (enabled)
        {
            if (Player != null && Agent.isOnNavMesh)
            {
                Agent.SetDestination(Player.position);
            }

            yield return Wait;
        }
    }
    
    private IEnumerator DoIdleMotion()
    {
        WaitForSeconds Wait = new WaitForSeconds(UpdateRate);
        Agent.speed *= IdleMovespeedMultiplier;
        
        while(true)
        {
            if (!Agent.enabled || !Agent.isOnNavMesh)
            {
                yield return Wait;
            }
            else if (Agent.remainingDistance <= Agent.stoppingDistance)
            {
                Vector2 point = Random.insideUnitCircle * IdleLocationRadius;
                NavMeshHit hit;

                if (NavMesh.SamplePosition(Agent.transform.position + new Vector3(point.x, 0, point.y), out hit, 2f,
                        Agent.areaMask))
                {
                    Agent.SetDestination(hit.position);
                }
            }

            yield return Wait;
        }
    }
}