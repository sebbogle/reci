namespace Tests.Utilities;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public sealed class ReciPageStateAttribute : Attribute
{
    public string? StateName { get; }

    public ReciPageStateAttribute()
    {
        StateName = null;
    }

    public ReciPageStateAttribute(string stateName)
    {
        ArgumentNullException.ThrowIfNull(stateName);
        StateName = stateName;
    }
}
