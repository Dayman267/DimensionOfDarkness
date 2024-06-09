using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Bullet Penetration Config", menuName = "Guns/Bullet Penetrartion Config", order = 6)]
public class BulletPenetrationConfigSO : ScriptableObject, ICloneable
{
    public int MaxObjectsToPenetrate;
    public float MaxPenetrationDepth = 0.275f;
    public Vector3 AccuracyLoss = new(0.1f, 0.1f, 0.1f);
    public float DamageRetentionPercentage;

    public object Clone()
    {
        var config = CreateInstance<BulletPenetrationConfigSO>();

        Utilities.CopyValues(this, config);

        return config;
    }
}