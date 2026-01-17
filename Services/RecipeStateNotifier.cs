namespace Reci.Services;

public class RecipeStateNotifier : IRecipeStateNotifier
{
    public event Action? OnRecipesChanged;

    public void NotifyRecipesChanged()
    {
        OnRecipesChanged?.Invoke();
    }
}
