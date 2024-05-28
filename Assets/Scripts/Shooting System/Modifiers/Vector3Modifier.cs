using UnityEngine;

public class Vector3Modifier : AbstractValueModifier<Vector3>
{
    public override void Apply(GunSO Gun)
    {
        try
        {
            var value = GetAttribute<Vector3>(Gun, out var targetObject, out var field);
            value = new Vector3(value.x * Amount.x, value.y * Amount.y, value.z * Amount.z);
            field.SetValue(targetObject, value);
        }
        catch (InvalidPathSpecifiedException)
        {
        } // dont kill thw flow, just log those errors
        // so we can fix them!
    }
}