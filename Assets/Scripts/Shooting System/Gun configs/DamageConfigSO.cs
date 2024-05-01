using System;
using UnityEngine;
using UnityEngine.Serialization;
using static UnityEngine.ParticleSystem;
using Random = UnityEngine.Random;

[CreateAssetMenu(fileName = "Damage Config", menuName = "Guns/Damage Config", order = 1)]
public class DamageConfigSO : ScriptableObject, ICloneable
{
    public MinMaxCurve DamageCurve;
    
    [Header("Critical damage")] 
    public float CriticalChance = 0f;
    public float CriticalMultiplier = 0f;
    
    [Header("Charged shot")]
    [Tooltip("Only works with Prepared Shot enabled in ShootConfig")]
    public bool IsChargedShot = false;
    public float maxChargedDamageMultiplier = 2.0f;

    private void Reset()
    {
        DamageCurve.mode = ParticleSystemCurveMode.Curve;
    }

    public int GetDamage(float Distance = 0, float DamageLoosMultiplier = 1 , float ChargeDamageMultiplier = 1 )
    {
        Debug.Log("Damage conf " + ChargeDamageMultiplier);
        float normalDamage = Mathf.CeilToInt(DamageCurve.Evaluate(Distance, Random.value)) * ChargeDamageMultiplier  * DamageLoosMultiplier ; // Получаем обычный урон

        if (CriticalChance > 0 && Random.value <= CriticalChance)
        {
            normalDamage *= (CriticalMultiplier);
        }
        
        return Mathf.CeilToInt(normalDamage);
    }
    
    /*public int GetDamage(float Distance = 0, float DamageMultiplier = 1)
    {
        return Mathf.CeilToInt(
            DamageCurve.Evaluate(Distance, Random.value) * DamageMultiplier
        );
    }*/

    public object Clone()
    {
        DamageConfigSO config = CreateInstance<DamageConfigSO>();
        Utilities.CopyValues(this,config);
        return config;
    }
}
