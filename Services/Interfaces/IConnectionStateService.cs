namespace Reci.Services.Interfaces;

public interface IConnectionStateService
{
    bool? IsBrowserSupported { get; }

    bool IsConnected { get; }

    string? FolderName { get; }

    event Func<Task>? ConnectionStateChanged;

    Task<bool> InitializeAndCheckAsync();

    Task<bool> ConnectAsync();

    Task<bool> ReconnectAsync();

    Task DisconnectAsync();
}
