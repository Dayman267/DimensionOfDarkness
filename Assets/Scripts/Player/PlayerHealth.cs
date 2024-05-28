using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    [SerializeField] private float healthPoints;
    [SerializeField] private float maxHealthPoints = 100f;
    private Image bar;

    private void Start()
    {
        bar = FindObjectOfType<HealthBar>().GetComponent<Image>();
        healthPoints = maxHealthPoints;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        bar.fillAmount = healthPoints / maxHealthPoints;
    }

    public void RestoreHealth(float points)
    {
        healthPoints += points;
        if (healthPoints > maxHealthPoints) healthPoints = maxHealthPoints;
        UpdateHealthBar();
    }

    public void DamageToHealth(float damage)
    {
        healthPoints -= damage;
        if (healthPoints <= 0)
        {
            healthPoints = 0;
            Destroy(gameObject);
        }

        UpdateHealthBar();
    }
}