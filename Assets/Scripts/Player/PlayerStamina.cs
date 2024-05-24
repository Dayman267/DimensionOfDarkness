using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStamina : MonoBehaviour
{
    [SerializeField] private float staminaPoints;
    [SerializeField] private float maxStaminaPoints = 100f;
    [SerializeField] private float waitSecAfterSpending = 1f;
    [SerializeField] private float waitSecBetweenRestoring = 0.001f;

    private Image bar;

    private bool isSpentRecently;

    private void Start()
    {
        bar = FindObjectOfType<StaminaBar>().GetComponent<Image>();
        staminaPoints = maxStaminaPoints;
        bar.fillAmount = staminaPoints / 100;
        StartCoroutine(RecoveryByTime());
    }

    public float GetStaminaPoints()
    {
        return staminaPoints;
    }

    private void ChangeStaminaBar(float staminaPoints)
    {
        bar.fillAmount = staminaPoints / 100;
    }

    public void RestoreStamina(float points)
    {
        staminaPoints += points;
        ChangeStaminaBar(staminaPoints);
    }

    public void SpendStamina(float points)
    {
        isSpentRecently = true;
        staminaPoints -= points;
        ChangeStaminaBar(staminaPoints);
    }

    private IEnumerator RecoveryByTime()
    {
        while (true)
        {
            while (staminaPoints < maxStaminaPoints)
            {
                if (isSpentRecently)
                {
                    yield return new WaitForSeconds(waitSecAfterSpending);
                    isSpentRecently = false;
                }

                RestoreStamina(1);
                yield return new WaitForSeconds(waitSecBetweenRestoring);
            }

            yield return null;
        }
    }
}