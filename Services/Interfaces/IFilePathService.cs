namespace Reci.Services.Interfaces;

public interface IFilePathService
{
    string SanitizeForFileSystem(string name);
    string ToFileName(string recipeName);
    string? ValidateFileName(string? name);
    string? ValidateGroupName(string? group);
    string? ValidateRecipePath(string? name, string? group);
}
