using System;
using System.Reflection;

public abstract class AbstractValueModifier<T> : IModifier
{
    public string AttributeName;
    public T Amount;

    public abstract void Apply(GunSO Gun);

    protected FieldType GetAttribute<FieldType>(
        GunSO Gun,
        out object TargetObject,
        out FieldInfo Field)
    {
        string[] paths = AttributeName.Split("/");
        string attribute = paths[paths.Length - 1];

        Type type = Gun.GetType();
        object target = Gun;

        for (int i = 0; i < paths.Length - 1; i++)
        {
            FieldInfo field = type.GetField(paths[i]);
            if (field == null)
            {
                UnityEngine.Debug.LogError($"Unable to apply modifier" +
                                           $" to attribute {AttributeName} because it does not exist on gun {Gun}");
                throw new InvalidPathSpecifiedException(AttributeName);
            }
            else
            {
                target = field.GetValue(target);
                type = target.GetType();
            }
        }

        FieldInfo attributeField = type.GetField(attribute);
        if (attributeField == null)
        {
            UnityEngine.Debug.LogError($"Unable to apply modifier to attribute " +
                                       $"{AttributeName} because it does not exist on gun {Gun}");
            throw new InvalidPathSpecifiedException(AttributeName);
        }

        Field = attributeField;
        TargetObject = target;
        return (FieldType)attributeField.GetValue(target);
    }
}