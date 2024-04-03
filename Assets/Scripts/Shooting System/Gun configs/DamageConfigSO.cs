using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigSO : ScriptableObject, ICloneable
{
    public MinMaxCurve DamageCurve;
    public float CriticalChance = 0f;
    public float CriticalMultiplier = 0f;

    private void Reset()
    {
        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(float Distance = 0)
    {
        float normalDamage = DamageCurve.Evaluate(Distance, UnityEngine.Random.value); // Получаем обычный урон
        float finalDamage = normalDamage;
        
        if (Random.value <= CriticalChance)
        {
           
            normalDamage *= (CriticalMultiplier);
        }
        
        return Mathf.CeilToInt(normalDamage);
    }

    public object Clone()
    {
        DamageConfigSO config = CreateInstance<DamageConfigSO>();
        config.DamageCurve = DamageCurve;
        config.CriticalChance = CriticalChance;
        config.CriticalMultiplier = CriticalMultiplier;
        return config;
    }
}
