using UnityEngine;

public class Frost : AbstractAreaOfEffect
{
    public AnimationCurve SlowDecay;

    public Frost(float Radius, AnimationCurve DamageFalloff, int BaseDamage, int MaxEnemiesAffected)
        : base(Radius, DamageFalloff, BaseDamage, MaxEnemiesAffected)
    {
        SlowDecay = new AnimationCurve();
    }

    public Frost(float Radius, AnimationCurve DamageFalloff, int BaseDamage, int MaxEnemiesAffected,
        AnimationCurve SlowDecay)
        : base(Radius, DamageFalloff, BaseDamage, MaxEnemiesAffected)
    {
        this.SlowDecay = SlowDecay;
    }

    public override void HandleImpact(Collider ImpactedObject, Vector3 HitPosition, Vector3 HitNormal, GunSO Gun)
    {
        base.HandleImpact(ImpactedObject, HitPosition, HitNormal, Gun);

        for (var i = 0; i < Hits; i++)
            if (HitObjects[i].TryGetComponent(out ISlowable slowable))
                slowable.Slow(SlowDecay);
    }
}