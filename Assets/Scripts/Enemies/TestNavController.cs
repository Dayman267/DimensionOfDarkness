using UnityEngine;
using UnityEngine.AI;

public class TestNavController : MonoBehaviour
{
    public NavMeshAgent agent;


    // Update is called once per frame
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            var movePosition = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(movePosition, out var hitInfo)) agent.SetDestination(hitInfo.point);
        }
    }
}