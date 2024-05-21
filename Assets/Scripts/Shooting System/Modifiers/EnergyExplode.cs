using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyExplode : AbstractAreaOfEffect
{
    public EnergyExplode(float Radius, AnimationCurve DamageFalloff, int BaseDamage, int MaxEnemiesAffected) : 
        base(Radius, DamageFalloff, BaseDamage, MaxEnemiesAffected){ }
}
