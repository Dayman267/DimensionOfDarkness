using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunModifiersApplier : MonoBehaviour
{
    [SerializeField] private PlayerGunSelector GunSelector;
    
    private void Start()
    {
        DamageModifier damageModifier = new()
        {
            Amount = 1f,
            AttributeName = "DamageConfig/DamageCurve"
        };
        damageModifier.Apply(GunSelector.ActiveGun);

        /*Vector3Modifier spreadModifier = new()
        {
            Amount = Vector3.zero,
            AttributeName = "ShootConfig/Spread"
        };
        spreadModifier.Apply(GunSelector.ActiveGun);*/
    }
}
