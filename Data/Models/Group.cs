namespace Reci.Data.Models;

public class Group
{
    public required Guid Id { get; set; }

    public required string Name { get; set; }

    public required int SortOrder { get; set; }

    public required GroupType GroupType { get; set; }
}

public enum GroupType
{
    Recipe,
    Ingredient,
    Instruction,
}

public static class GroupExtensions
{
    public static bool IsEmpty(this Group? group)
    {
        if (group is null)
        {
            return true;
        }
        return group.Id == Guid.Empty
            && string.IsNullOrEmpty(group.Name)
            && group.SortOrder == 0;
    }

    public static bool IsEqualTo(this Group? first, Group? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Id == second.Id
            && first.Name == second.Name
            && first.SortOrder == second.SortOrder
            && first.GroupType == second.GroupType;
    }
}
