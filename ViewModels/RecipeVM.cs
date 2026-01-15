namespace Reci.ViewModels;

public class RecipeVM
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public Group? Group { get; set; }

    public List<GroupVM<Ingredient>> Ingredients { get; set; } = [];

    public List<GroupVM<Instruction>> Instructions { get; set; } = [];

    public NutritionInfo? NutritionInfo { get; set; }

    public List<string> Tags { get; set; } = [];

    public string? FurtherNotes { get; set; }
}
