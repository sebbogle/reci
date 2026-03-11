namespace Reci.Data.Models;

public record Ingredient : IGroupable
{
    public required string Name { get; set; }

    public required decimal QuantityAmount { get; set; }

    public required string QuantityUnit { get; set; }

    public string? Group { get; set; }
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
            && ingredient.Group is null;
    }

}
