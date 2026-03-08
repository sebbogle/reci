namespace Reci.Core;

public static class IListExtensions
{
    public static bool Replace<T>(this IList<T> source, T newItem, Func<T, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(predicate);

        for (int i = 0; i < source.Count; i++)
        {
            if (predicate(source[i]))
            {
                source[i] = newItem;
                return true;
            }
        }
        return false;
    }


}
