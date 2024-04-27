using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explode : AbstractAreaOfEffect
{
    public Explode(float Radius, AnimationCurve DamageFalloff, int BaseDamage, int MaxEnemiesAffected) : 
        base(Radius, DamageFalloff, BaseDamage, MaxEnemiesAffected){ }
}