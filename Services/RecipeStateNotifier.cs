namespace Reci.Services;

public class RecipeStateNotifier : IRecipeStateNotifier
{
    public event Func<Task>? OnRecipesChanged;

    public async Task NotifyRecipesChangedAsync()
    {
        Func<Task>? handler = OnRecipesChanged;
        if (handler is not null)
        {
            Delegate[] delegates = handler.GetInvocationList();
            foreach (Delegate del in delegates)
            {
                Func<Task> func = (Func<Task>)del;
                await func().ConfigureAwait(false);
            }
        }
    }
}
