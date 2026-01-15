using Microsoft.JSInterop;
using Reci.Services.Interfaces;
using System.Collections.Concurrent;

namespace Reci.Services;

public class ScreenWakeLockService(IJSRuntime jsRuntime, ILogger<ScreenWakeLockService> logger) : IScreenWakeLockService
{
    private readonly ILogger<ScreenWakeLockService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IJSRuntime _jsRuntime = jsRuntime ?? throw new ArgumentNullException(nameof(jsRuntime));
    private readonly ConcurrentDictionary<int, WakeLockSentinel> _wakeLocks = new ConcurrentDictionary<int, WakeLockSentinel>();
    
    private int _nextId = 0;

    public async Task<WakeLockSentinel?> RequestWakeLockAsync()
    {
        bool isSupported = await IsSupportedAsync();
        if (!isSupported)
        {
            _logger.LogInformation("Wake Lock requested on unsupported browser");
            return null;
        }

        IJSObjectReference jsObjectReference = await _jsRuntime.InvokeAsync<IJSObjectReference>("navigator.wakeLock.request", "screen");

        int id = Interlocked.Increment(ref _nextId);
        WakeLockSentinel sentinel = new ()
        {
            Id = id,
            JsObjectReference= jsObjectReference 
        };
        _wakeLocks.TryAdd(id, sentinel);

        return sentinel;
    }

    public async Task ReleaseWakeLockAsync(WakeLockSentinel sentinel)
    {
        ArgumentNullException.ThrowIfNull(sentinel);

        await sentinel.JsObjectReference.InvokeVoidAsync("release");
        await sentinel.JsObjectReference.DisposeAsync();

        _wakeLocks.TryRemove(sentinel.Id, out _);
    }

    public async Task<bool> IsSupportedAsync() 
        => await _jsRuntime.InvokeAsync<bool>("eval", "typeof navigator.wakeLock !== 'undefined'");
    
}
