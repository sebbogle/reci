namespace Reci.Services.Interfaces;

public interface IScreenWakeLockService : IAsyncDisposable
{
    Task<WakeLockSentinel?> RequestWakeLockAsync();

    Task ReleaseWakeLockAsync(WakeLockSentinel sentinel);

    Task<bool> IsSupportedAsync();
}
