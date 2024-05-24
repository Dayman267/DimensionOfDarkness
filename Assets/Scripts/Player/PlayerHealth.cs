using UnityEngine;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour, IDamageable
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

    public int CurrentHealth { get; }
    public int MaxHealth { get; }
    public event IDamageable.TakeDamageEvent OnTakeDamage;
    public event IDamageable.DeathEvent OnDeath;

    public void TakeDamage(int damage)
    {
        DamageToHealth(damage);
    }

    public Transform GetTransform()
    {
        return transform;
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