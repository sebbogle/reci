namespace Reci.Services.Interfaces;

public interface IScreenWakeLockService
{
    Task<WakeLockSentinel?> RequestWakeLockAsync();

    Task ReleaseWakeLockAsync(WakeLockSentinel sentinel);

    Task<bool> IsSupportedAsync();
}
