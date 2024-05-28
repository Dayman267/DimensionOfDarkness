public class ImpactTypeModifier : AbstractValueModifier<ImpactType>
{
    public override void Apply(GunSO Gun)
    {
        Gun.ImpactType = Amount;
    }
}