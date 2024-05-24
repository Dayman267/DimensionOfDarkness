using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class PlayerShield : MonoBehaviour
{
    [SerializeField] private float shieldPoints;
    [SerializeField] private float maxShieldPoints = 100f;
    
    private Image bar;

    private PlayerHealth playerHealth;
    
    private bool isDamagedRecently = false;
    [SerializeField] private float waitSecAfterDamage = 2f;
    [SerializeField] private float waitSecBetweenRestoring = 0.1f;
    
    private void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
        bar = FindObjectOfType<ShieldBar>().GetComponent<Image>();
        shieldPoints = maxShieldPoints;
        bar.fillAmount = shieldPoints/100;
        StartCoroutine(RecoveryByTime());
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            DamageToShield(20f);
        }
    
        if (Input.GetKeyDown(KeyCode.F))
        {
            RestoreShield(20f);
        }
    }

    private void ChangeShieldBar(float shieldPoints)
    {
        bar.fillAmount = shieldPoints/100;
    }
    
    private void RestoreShield(float points)
    {
        if (shieldPoints < 100)
        {
            shieldPoints += points;
            ChangeShieldBar(shieldPoints);
        }
    }
    
    public void DamageToShield(float damage)
    {
        isDamagedRecently = true;
        if (shieldPoints - damage >= 0)
        {
            shieldPoints -= damage;
            ChangeShieldBar(shieldPoints);
        }
        else if (shieldPoints > 0 && shieldPoints - damage < 0)
        {
            float remainder = damage - shieldPoints;
            shieldPoints = 0;
            ChangeShieldBar(shieldPoints);
            playerHealth.DamageToHealth(remainder);
        }
        else
        {
            shieldPoints = 0;
            ChangeShieldBar(shieldPoints);
            playerHealth.DamageToHealth(damage);
        }
    }

    private IEnumerator RecoveryByTime()
    {
        while (true)
        {
            while (shieldPoints < maxShieldPoints)
            {
                if (isDamagedRecently)
                {
                    yield return new WaitForSeconds(waitSecAfterDamage);
                    isDamagedRecently = false;
                }

                float targetShieldPoints = shieldPoints + 1;
                float duration = waitSecBetweenRestoring; // время восстановления
                float elapsed = 0;

                while (elapsed < duration)
                {
                    shieldPoints = Mathf.Lerp(shieldPoints, targetShieldPoints, elapsed / duration);
                    ChangeShieldBar(shieldPoints);
                    elapsed += Time.deltaTime;
                    yield return null;
                }

                shieldPoints = targetShieldPoints;
            }
            yield return null;
        }
    }
}