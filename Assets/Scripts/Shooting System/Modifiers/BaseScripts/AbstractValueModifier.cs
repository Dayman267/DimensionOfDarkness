using System.Reflection;
using UnityEngine;

public abstract class AbstractValueModifier<T> : IModifier
{
    public T Amount;
    public string AttributeName;

    public abstract void Apply(GunSO Gun);

    protected FieldType GetAttribute<FieldType>(
        GunSO Gun,
        out object TargetObject,
        out FieldInfo Field)
    {
        var paths = AttributeName.Split("/");
        var attribute = paths[paths.Length - 1];

        var type = Gun.GetType();
        object target = Gun;

        for (var i = 0; i < paths.Length - 1; i++)
        {
            var field = type.GetField(paths[i]);
            if (field == null)
            {
                Debug.LogError("Unable to apply modifier" +
                               $" to attribute {AttributeName} because it does not exist on gun {Gun}");
                throw new InvalidPathSpecifiedException(AttributeName);
            }

            target = field.GetValue(target);
            type = target.GetType();
        }

        var attributeField = type.GetField(attribute);
        if (attributeField == null)
        {
            Debug.LogError("Unable to apply modifier to attribute " +
                           $"{AttributeName} because it does not exist on gun {Gun}");
            throw new InvalidPathSpecifiedException(AttributeName);
        }

        Field = attributeField;
        TargetObject = target;
        return (FieldType)attributeField.GetValue(target);
    }
}