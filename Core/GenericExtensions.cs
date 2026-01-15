using System.Runtime.CompilerServices;

namespace Reci.Core;

public static class GenericExtensions
{
    public static T ThrowIfNull<T>(this T? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
    where T : class
    {
        ArgumentNullException.ThrowIfNull(argument, paramName);
        return argument;
    }
}
