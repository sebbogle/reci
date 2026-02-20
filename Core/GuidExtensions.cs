namespace Reci.Core;

public static class GuidExtensions
{
    public static Guid PopulateIfEmpty(this Guid guid)
    {
        return guid == Guid.Empty ? Guid.NewGuid() : guid;
    }
}
