namespace Reci.Services.Interfaces;

public interface IRecipeStateNotifier
{
    event Func<Task>? OnRecipesChanged;

    Task NotifyRecipesChangedAsync();
}
