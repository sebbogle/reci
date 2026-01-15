namespace Reci.Mappers;

public static class RecipeMapper
{
    private const int DefaultSortOrder = 0;

    public static List<GroupVM<RecipeSummaryVM>> ToViewModelGroups(this List<Recipe> recipes, List<Group> groups)
    {
        Dictionary<Guid, Group> groupsById = groups?.ToDictionary(g => g.Id) ?? [];

        List<GroupVM<RecipeSummaryVM>> groupedRecipes = recipes.GroupBy(r => r.GroupId)
                                    .Select(g => CreateRecipeSummaryGroup(g, groupsById))
                                    .OrderBy(g => g.SortOrder)
                                    .ToList();

        return groupedRecipes;
    }

    public static RecipeVM ToViewModel(this Recipe recipe, List<Group> groups)
    {
        Dictionary<Guid, Group> groupsById = groups?.ToDictionary(g => g.Id) ?? [];

        List<GroupVM<Ingredient>> ingredientsGroup = GroupItems(recipe.Ingredients, i => i.GroupId, groupsById);
        List<GroupVM<Instruction>> instructionGroups = GroupItems(recipe.Instructions, i => i.GroupId, groupsById);

        return new RecipeVM
        {
            Id = recipe.Id,
            Name = recipe.Name,
            Description = recipe.Description,
            Group = recipe.GroupId.HasValue && groupsById.TryGetValue(recipe.GroupId.Value, out Group? group)
                ? group
                : null,
            Ingredients = ingredientsGroup,
            Instructions = instructionGroups,
            NutritionInfo = recipe.NutritionInfo,
            Tags = recipe.Tags,
            FurtherNotes = recipe.FurtherNotes
        };
    }

    public static Recipe ToModel(this RecipeVM recipeVM)
    {
        return new Recipe
        {
            Id = recipeVM.Id ?? Guid.Empty,
            Name = recipeVM.Name ?? string.Empty,
            Description = recipeVM.Description,
            GroupId = recipeVM.Group?.Id,
            Ingredients = recipeVM.Ingredients.SelectMany(g => g).ToList(),
            Instructions = recipeVM.Instructions.SelectMany(g => g).ToList(),
            NutritionInfo = recipeVM.NutritionInfo,
            Tags = recipeVM.Tags,
            FurtherNotes = recipeVM.FurtherNotes
        };
    }

    private static GroupVM<RecipeSummaryVM> CreateRecipeSummaryGroup(
        IGrouping<Guid?, Recipe> recipeGroup,
        Dictionary<Guid, Group> groupsById)
    {
        Group? group = recipeGroup.Key.HasValue && groupsById.TryGetValue(recipeGroup.Key.Value, out Group? g)
            ? g
            : null;

        GroupVM<RecipeSummaryVM> groupVM = new()
        {
            Id = group?.Id,
            Name = group?.Name,
            SortOrder = group?.SortOrder ?? DefaultSortOrder
        };

        groupVM.AddRange(recipeGroup.Select(r => new RecipeSummaryVM
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            Tags = r.Tags
        }));

        return groupVM;
    }

    private static List<GroupVM<T>> GroupItems<T>(
        List<T> items,
        Func<T, Guid?> groupIdSelector,
        Dictionary<Guid, Group> groupsById)
    {
        return items.GroupBy(groupIdSelector)
                    .Select(g =>
                    {
                        Group? group = g.Key.HasValue && groupsById.TryGetValue(g.Key.Value, out Group? grp)
                            ? grp
                            : null;

                        GroupVM<T> groupVM = new()
                        {
                            Id = group?.Id,
                            Name = group?.Name,
                            SortOrder = group?.SortOrder ?? DefaultSortOrder
                        };

                        groupVM.AddRange(g);

                        return groupVM;
                    })
                    .OrderBy(gr => gr.SortOrder)
                    .ToList();
    }
}
