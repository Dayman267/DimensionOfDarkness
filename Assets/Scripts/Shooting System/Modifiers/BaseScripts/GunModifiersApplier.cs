using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunModifiersApplier : MonoBehaviour
{
    [SerializeField] private ImpactType ImpactTypeOverride;
    [SerializeField] private PlayerGunSelector GunSelector;

    private void Start()
    {
        /*new ImpactTypeModifier()
        {
            Amount = ImpactTypeOverride
        }.Apply(GunSelector.ActiveGun);*/
        
        new ImpactTypeModifier()
        {
            Amount = GunSelector.ActiveGun.ImpactType
        }.Apply(GunSelector.ActiveGun);

        
        ApplyModifiers();
        

        /*GunSelector.ActiveGun.BulletImpactEffects = new ICollisionHandler[]
        {
            new Frost(
                1.5f,
                new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(0, 1), 
                    new Keyframe(1, 0.25f)
                }),
                10,
                20,
                new AnimationCurve(new Keyframe[]
                {
                    new Keyframe(0, 0.25f),
                    new Keyframe(1.75f, 0.25f),
                    new Keyframe(2,1)
                })
            )
        };*/
    }

    private void ApplyModifiers()
    {
        if (GunSelector.ActiveGun.Type == GunType.GrandeLauncher)
        {
            GunSelector.ActiveGun.BulletImpactEffects = new ICollisionHandler[]
            {
                new Explode(
                    2f,
                    new AnimationCurve(new Keyframe[] { new Keyframe(0, 1), new Keyframe(1, 0.25f) }),
                    15,
                    20
                )
            };
        }
    }

    private void OnEnable()
    {
        PlayerGunSelector.OnGunChanged += ApplyModifiers;
    }

    private void OnDisable()
    {
        PlayerGunSelector.OnGunChanged -= ApplyModifiers;
    }
}