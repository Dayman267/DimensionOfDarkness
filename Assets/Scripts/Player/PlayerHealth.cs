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
        bar.fillAmount = healthPoints/100;
    }
    
    private void ChangeHealthBar(float points)
    {
        bar.fillAmount = points/100;
    }
    
    public void RestoreHealth(float points)
    {
        healthPoints += points;
        ChangeHealthBar(healthPoints);
    }
    
    public void DamageToHealth(float damage)
    {
        healthPoints -= damage;
        ChangeHealthBar(healthPoints);
        if (healthPoints <= 0)
        {
            Destroy(gameObject);
        }
    }
}
