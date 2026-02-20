namespace Reci.Core;

public static class GuidExtensions
{
    public static bool PopulateIfEmpty(this Guid guid)
    {
        if (guid == Guid.Empty)
        {
            guid = Guid.NewGuid();
            return true;
        }
        return false;
    }
}
