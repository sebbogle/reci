namespace Reci.Services;

public class RecipeStateNotifier : IRecipeStateNotifier
{
    private readonly List<Func<Task>> _callbacks = new();

    public void Subscribe(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callbacks.Add(callback);
    }

    public void Unsubscribe(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callbacks.Remove(callback);
    }

    public async Task NotifyRecipesChangedAsync()
    {
        foreach (Func<Task> callback in _callbacks)
        {
            try
            {
                await callback();
            }
            catch (Exception)
            {
                // Individual handler failure should not prevent other handlers from executing.
            }
        }
    }
}
