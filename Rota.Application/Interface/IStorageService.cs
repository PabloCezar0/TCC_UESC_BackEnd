namespace Rota.Application.Common.Storage;

public interface IStorageService
{
    Task<string> UploadAsync(Stream content, string key, string contentType,
                             CancellationToken ct = default);
    Task<Stream> DownloadAsync(string key, CancellationToken ct = default);
    Task DeleteAsync(string key, CancellationToken ct = default);
}
