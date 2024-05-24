using UnityEngine;
using static UnityEngine.ParticleSystem;

public class DamageModifier : AbstractValueModifier<float>
{
    public override void Apply(GunSO Gun)
    {
        try
        {
            var damageCurve = GetAttribute<MinMaxCurve>(
                Gun,
                out var targetObject,
                out var field);

            switch (damageCurve.mode)
            {
                case ParticleSystemCurveMode.TwoConstants:
                    damageCurve.constantMin *= Amount;
                    damageCurve.constantMax *= Amount;
                    break;
                case ParticleSystemCurveMode.TwoCurves:
                    damageCurve.curveMultiplier *= Amount;
                    break;
                case ParticleSystemCurveMode.Curve:
                    damageCurve.curveMultiplier *= Amount;
                    break;
                case ParticleSystemCurveMode.Constant:
                    damageCurve.constant *= Amount;
                    break;
            }

            field.SetValue(targetObject, damageCurve);
        }
        catch (InvalidPathSpecifiedException)
        {
        } // dont kill thw flow, just log those errors
        // so we can fix them!
    }
}