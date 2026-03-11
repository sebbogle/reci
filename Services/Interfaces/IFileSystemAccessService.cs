namespace Reci.Services.Interfaces;

public interface IFileSystemAccessService
{
    Task<bool> IsFileSystemAccessSupportedAsync();

    Task<string?> PickDirectoryAsync();

    Task<bool> HasStoredDirectoryAsync();

    Task<string?> ReconnectDirectoryAsync();

    Task DisconnectDirectoryAsync();

    bool IsConnected();

    string? GetDirectoryName();

    Task<List<FileSystemEntry>> ListEntriesAsync(string? subfolder = null);

    Task<string> ReadFileAsync(string path);

    Task WriteFileAsync(string path, string content);

    Task DeleteFileAsync(string path);

    Task CreateDirectoryAsync(string name);

    Task DeleteDirectoryAsync(string name);

    Task MoveFileAsync(string fromPath, string toPath);

    Task RenameDirectoryAsync(string oldName, string newName);

    Task DownloadBlobAsync(string filename, byte[] content, string mimeType);
}

public class FileSystemEntry
{
    public required string Name { get; init; }

    public required string Kind { get; init; }

    public bool IsFile => Kind == "file";

    public bool IsDirectory => Kind == "directory";
}
