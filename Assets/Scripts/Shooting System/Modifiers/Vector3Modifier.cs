using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public class Vector3Modifier : AbstractValueModifier<Vector3>
{
    public override void Apply(GunSO Gun)
    {
        try
        {
            Vector3 value = GetAttribute<Vector3>(Gun, out object targetObject, out FieldInfo field);
            value = new(value.x * Amount.x, value.y * Amount.y, value.z * Amount.z);
            field.SetValue(targetObject, value);
        }
        catch (InvalidPathSpecifiedException) { }// dont kill thw flow, just log those errors
        // so we can fix them!
    }
}
