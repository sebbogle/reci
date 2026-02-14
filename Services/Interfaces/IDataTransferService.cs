namespace Reci.Services.Interfaces;

public interface IDataTransferService
{
    public Task<Result> ImportReciDefinitionAsync(ReciFile reciFile, CancellationToken cancellationToken = default);

    public Task<ReciFile?> ExportReciDefinitionAsync(CancellationToken cancellationToken = default);

    public string MendGuidsFromImportedData(string data);
}
