public class Utilities
{
    public static void CopyValues<T>(T Base, T Copy)
    {
        var type = Base.GetType();
        foreach (var field in type.GetFields()) field.SetValue(Copy, field.GetValue(Base));
    }
}