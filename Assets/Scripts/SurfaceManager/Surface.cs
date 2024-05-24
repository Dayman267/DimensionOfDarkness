using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Impact System/Surface", fileName = "Surface")]
public class Surface : ScriptableObject
{
    public List<SurfaceImpactTypeEffect> ImpactTypeEffects = new();

    [Serializable]
    public class SurfaceImpactTypeEffect
    {
        public ImpactType ImpactType;
        public SurfaceEffect SurfaceEffect;
    }
}