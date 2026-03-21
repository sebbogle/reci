namespace Reci.Services.Interfaces;

public record RecipeExportResult(byte[] Content, string FileName);

public interface IRecipeExportService
{
    Task<byte[]?> ExportAllAsZipAsync(CancellationToken cancellationToken = default);

    Task<byte[]?> ExportGroupAsZipAsync(string groupName, CancellationToken cancellationToken = default);

    Task<RecipeExportResult?> ExportRecipeAsync(RecipeKey key, CancellationToken cancellationToken = default);
}
