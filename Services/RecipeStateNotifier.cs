namespace Reci.Services;

public class RecipeStateNotifier(ILogger<RecipeStateNotifier> logger) : IRecipeStateNotifier
{
    private readonly ILogger<RecipeStateNotifier> _logger = logger.ThrowIfNull();
    private readonly List<Func<Task>> _callbacks = new();

    public void Subscribe(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callbacks.Add(callback);
        _logger.LogDebug("Subscriber added. Total subscribers: {SubscriberCount}", _callbacks.Count);
    }

    public void Unsubscribe(Func<Task> callback)
    {
        ArgumentNullException.ThrowIfNull(callback);
        _callbacks.Remove(callback);
        _logger.LogDebug("Subscriber removed. Total subscribers: {SubscriberCount}", _callbacks.Count);
    }

    public async Task NotifyRecipesChangedAsync()
    {
        _logger.LogDebug("Notifying {SubscriberCount} subscribers of recipe changes", _callbacks.Count);
        foreach (Func<Task> callback in _callbacks)
        {
            try
            {
                await callback();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying subscriber of recipe changes");
            }
        }
    }
}
