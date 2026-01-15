namespace Reci.Data.Models;

using Reci.Interfaces;

public class Ingredient : IGroupable
{
    public required string Name { get; set; }

    public required decimal QuantityAmount { get; set; }

    public required string QuantityUnit { get; set; }

    public Guid? GroupId { get; set; }
}

public static class IngredientExtensions
{
    public static bool IsEmpty(this Ingredient? ingredient)
    {
        if (ingredient is null)
        {
            return true;
        }
        return string.IsNullOrEmpty(ingredient.Name)
            && ingredient.QuantityAmount == 0
            && string.IsNullOrEmpty(ingredient.QuantityUnit)
            && ingredient.GroupId is null;
    }

    public static bool IsEqualTo(this Ingredient? first, Ingredient? second)
    {
        if (first is null && second is null)
        {
            return true;
        }
        if (first is null || second is null)
        {
            return false;
        }
        return first.Name == second.Name
            && first.QuantityAmount == second.QuantityAmount
            && first.QuantityUnit == second.QuantityUnit
            && first.GroupId == second.GroupId;
    }
}
