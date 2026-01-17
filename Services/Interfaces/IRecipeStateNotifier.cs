namespace Reci.Services.Interfaces;

public interface IRecipeStateNotifier
{
    event Action? OnRecipesChanged;

    void NotifyRecipesChanged();
}
