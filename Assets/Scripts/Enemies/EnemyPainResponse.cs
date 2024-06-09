using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(Animator))]
public class EnemyPainResponse : MonoBehaviour
{
    [SerializeField] private EnemyHealth Health;

    [SerializeField] [Range(1, 100)] private int MaxDamagePainThreshold = 5;

    private Animator Animator;

    private void Awake()
    {
        Animator = GetComponent<Animator>();
    }

    public void HandlePain(int Damage)
    {
        if (Health.CurrentHealth > 0)
        {
            Animator.SetTrigger("Hit");
        }
    }

    public void HandleDeath()
    {
        Animator.applyRootMotion = true;
        Animator.SetTrigger("Die");
        Animator.SetBool("isDead",true);
    }
}