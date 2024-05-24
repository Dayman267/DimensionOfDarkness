using System;
using UnityEngine;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigSO : ScriptableObject, ICloneable
{
    public MinMaxCurve DamageCurve;
    public float CriticalChance;
    public float CriticalMultiplier;

    private void Reset()
    {
        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public object Clone()
    {
        var config = CreateInstance<DamageConfigSO>();
        Utilities.CopyValues(this, config);
        return config;
    }

    /*public int GetDamage(float Distance = 0, float DamageMultiplier = 1 )
    {
        float normalDamage = Mathf.CeilToInt(DamageCurve.Evaluate(Distance, Random.value)) * DamageMultiplier; // Получаем обычный урон

        if (CriticalChance > 0 && Random.value <= CriticalChance)
        {
            normalDamage *= (CriticalMultiplier);
        }

        return Mathf.CeilToInt(normalDamage);
    }*/

    public int GetDamage(float Distance = 0, float DamageMultiplier = 1)
    {
        return Mathf.CeilToInt(
            DamageCurve.Evaluate(Distance, Random.value) * DamageMultiplier
        );
    }
}