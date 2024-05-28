using System.Collections;
using TMPro;
using UnityEngine;

public class PointsManager : MonoBehaviour
{
    public int darkEnergyPoints;
    public int darkEnergyPointsToAccess;
    public int pointsPerTime;
    public int secondsBetweenAward;

    public int firstFibonacci;
    public int secondFibonacci;

    public TextMeshProUGUI darkEnergyText;

    public int counterICanEnter;

    private void Start()
    {
        firstFibonacci = secondFibonacci = darkEnergyPointsToAccess;
        StartCoroutine(DarkEnergyTimeIncreaser());
    }

    public void RpcAddDarkEnergyPoints(int points)
    {
        darkEnergyPoints += points;
        if (darkEnergyPoints >= darkEnergyPointsToAccess)
        {
            darkEnergyPointsToAccess = firstFibonacci + secondFibonacci;
            secondFibonacci = firstFibonacci;
            firstFibonacci = darkEnergyPointsToAccess;
            counterICanEnter += 1;
        }
    }

    public void RpcSubtractOneCounterICanEnter()
    {
        counterICanEnter -= 1;
    }

    private IEnumerator DarkEnergyTimeIncreaser()
    {
        while (true)
        {
            RpcAddDarkEnergyPoints(pointsPerTime);
            yield return new WaitForSeconds(secondsBetweenAward);
        }
    }
}