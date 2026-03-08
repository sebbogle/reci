namespace Reci.Services;

public class FileSystemAccessService(IJSRuntime jsRuntime, ILogger<FileSystemAccessService> logger) : IFileSystemAccessService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime.ThrowIfNull();
    private readonly ILogger<FileSystemAccessService> _logger = logger.ThrowIfNull();

    private string? _directoryName;

    public async Task<bool> IsFileSystemAccessSupportedAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("fileSystem.isSupported");
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error checking File System Access API support");
            return false;
        }
    }

    public async Task<string?> PickDirectoryAsync()
    {
        try
        {
            string? name = await _jsRuntime.InvokeAsync<string?>("fileSystem.pickDirectory");
            _directoryName = name;
            if (name is not null)
            {
                _logger.LogInformation("Directory picked: {DirectoryName}", name);
            }
            return name;
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error picking directory");
            return null;
        }
    }

    public async Task<bool> HasStoredDirectoryAsync()
    {
        try
        {
            return await _jsRuntime.InvokeAsync<bool>("fileSystem.hasStoredDirectory");
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error checking for stored directory");
            return false;
        }
    }

    public async Task<string?> ReconnectDirectoryAsync()
    {
        try
        {
            string? name = await _jsRuntime.InvokeAsync<string?>("fileSystem.reconnectDirectory");
            _directoryName = name;
            if (name is not null)
            {
                _logger.LogInformation("Reconnected to directory: {DirectoryName}", name);
            }
            return name;
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error reconnecting directory");
            return null;
        }
    }

    public async Task DisconnectDirectoryAsync()
    {
        try
        {
            await _jsRuntime.InvokeVoidAsync("fileSystem.disconnectDirectory");
            _directoryName = null;
            _logger.LogInformation("Directory disconnected");
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error disconnecting directory");
        }
    }

    public bool IsConnected()
    {
        return _directoryName is not null;
    }

    public string? GetDirectoryName()
    {
        return _directoryName;
    }

    public async Task<List<FileSystemEntry>> ListEntriesAsync(string? subfolder = null)
    {
        try
        {
            List<FileSystemEntry> entries = await _jsRuntime.InvokeAsync<List<FileSystemEntry>>("fileSystem.listEntries", subfolder);
            return entries ?? [];
        }
        catch (JSException ex)
        {
            _logger.LogError(ex, "Error listing entries in {Subfolder}", subfolder ?? "root");
            return [];
        }
    }

    public async Task<string> ReadFileAsync(string path)
    {
        return await _jsRuntime.InvokeAsync<string>("fileSystem.readFile", path);
    }

    public async Task WriteFileAsync(string path, string content)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.writeFile", path, content);
    }

    public async Task DeleteFileAsync(string path)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.deleteFile", path);
    }

    public async Task CreateDirectoryAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.createDirectory", name);
    }

    public async Task DeleteDirectoryAsync(string name)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.deleteDirectory", name);
    }

    public async Task MoveFileAsync(string fromPath, string toPath)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.moveFile", fromPath, toPath);
    }

    public async Task RenameDirectoryAsync(string oldName, string newName)
    {
        await _jsRuntime.InvokeVoidAsync("fileSystem.renameDirectory", oldName, newName);
    }

    public async Task DownloadBlobAsync(string filename, byte[] content, string mimeType)
    {
        string base64 = Convert.ToBase64String(content);
        await _jsRuntime.InvokeVoidAsync("fileSystem.downloadBlob", filename, base64, mimeType);
    }
}
