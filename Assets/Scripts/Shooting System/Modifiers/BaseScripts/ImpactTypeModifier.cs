using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpactTypeModifier : AbstractValueModifier<ImpactType>
{
    public override void Apply(GunSO Gun)
    {
        Gun.ImpactType = Amount;
    }
}
