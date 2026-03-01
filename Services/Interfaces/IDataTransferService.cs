namespace Reci.Services.Interfaces;

public interface IDataTransferService
{
    Task<Result> ImportReciDefinitionAsync(ReciFile reciFile, CancellationToken cancellationToken = default);

    Task<ReciFile?> ExportReciDefinitionAsync(CancellationToken cancellationToken = default);

    string MendGuidsFromImportedData(string data);
}
