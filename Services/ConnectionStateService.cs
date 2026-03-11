namespace Reci.Services;

public class ConnectionStateService(IFileSystemAccessService fileSystemAccess, ILogger<ConnectionStateService> logger) : IConnectionStateService
{
    private readonly IFileSystemAccessService _fileSystemAccess = fileSystemAccess.ThrowIfNull();
    private readonly ILogger<ConnectionStateService> _logger = logger.ThrowIfNull();

    public bool? IsBrowserSupported { get; private set; }

    public bool IsConnected => _fileSystemAccess.IsConnected();

    public string? FolderName => _fileSystemAccess.GetDirectoryName();

    public event Func<Task>? ConnectionStateChanged;

    public async Task<bool> InitializeAndCheckAsync()
    {
        IsBrowserSupported = await _fileSystemAccess.IsFileSystemAccessSupportedAsync();
        if (IsBrowserSupported == false)
        {
            _logger.LogWarning("File System Access API is not supported in this browser");
            return false;
        }

        bool hasStored = await _fileSystemAccess.HasStoredDirectoryAsync();
        if (hasStored)
        {
            string? name = await _fileSystemAccess.ReconnectDirectoryAsync();
            if (name is not null)
            {
                _logger.LogInformation("Auto-reconnected to folder: {FolderName}", name);
                await NotifyStateChangedAsync();
                return true;
            }
            else
            {
                _logger.LogInformation("Stored directory found but permission not granted");
                return true;
            }
        }
        return false;
    }

    public async Task<bool> ConnectAsync()
    {
        string? name = await _fileSystemAccess.PickDirectoryAsync();
        if (name is not null)
        {
            _logger.LogInformation("Connected to folder: {FolderName}", name);
            await NotifyStateChangedAsync();
            return true;
        }
        return false;
    }

    public async Task<bool> ReconnectAsync()
    {
        string? name = await _fileSystemAccess.ReconnectDirectoryAsync();
        if (name is not null)
        {
            _logger.LogInformation("Reconnected to folder: {FolderName}", name);
            await NotifyStateChangedAsync();
            return true;
        }
        return false;
    }

    public async Task DisconnectAsync()
    {
        await _fileSystemAccess.DisconnectDirectoryAsync();
        _logger.LogInformation("Disconnected from folder");
        await NotifyStateChangedAsync();
    }

    private async Task NotifyStateChangedAsync()
    {
        if (ConnectionStateChanged is not null)
        {
            foreach (Func<Task> handler in ConnectionStateChanged.GetInvocationList().Cast<Func<Task>>())
            {
                try
                {
                    await handler();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error notifying connection state change");
                }
            }
        }
    }
}
