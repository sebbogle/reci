namespace Reci.Services.Interfaces;

public interface IRecipeStateNotifier
{
    void Subscribe(Func<Task> callback);

    void Unsubscribe(Func<Task> callback);

    Task NotifyRecipesChangedAsync();
}
