namespace Reci.Core;

public class WakeLockSentinel : IAsyncDisposable
{
    public required int Id { get; init; }

    public required IJSObjectReference JsObjectReference { get; init; }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await JsObjectReference.InvokeVoidAsync("release");
        }
        catch (JSDisconnectedException)
        {
            // JS runtime already disconnected, nothing to release.
        }

        try
        {
            await JsObjectReference.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
            // JS runtime already disconnected.
        }
    }
}
