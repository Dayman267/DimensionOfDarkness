using System;
using UnityEngine;

[CreateAssetMenu(fileName = "Trail Config", menuName = "Guns/Gun Trail Config", order = 4)]
public class TrailConfigSO : ScriptableObject, ICloneable
{
    public Material Material;
    public AnimationCurve WidthCurve;
    public float Duration = 0.5f;
    public float MinVertexDistance = 0.1f;
    public Gradient Color;
    public GameObject BulletParticlePrefab;

    public float MissDistance = 100f;
    public float SimulationSpeed = 100f;

    public object Clone()
    {
        var config = CreateInstance<TrailConfigSO>();

        Utilities.CopyValues(this, config);

        return config;
    }
}