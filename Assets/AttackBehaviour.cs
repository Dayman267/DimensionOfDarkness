using UnityEngine;

public class AttackBehaviour : StateMachineBehaviour
{
    private Transform player;

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
    }


    public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.transform.LookAt(player);
        var distance = Vector3.Distance(animator.transform.position, player.position);
        if (distance > 3)
            animator.SetBool("isAttacking", false);
    }


    // override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    // {
    //     
    // }
}