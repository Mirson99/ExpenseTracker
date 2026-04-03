namespace ExpenseTracker.Application.Interfaces;

public interface IFileStorageService
{
    Task<string> UploadFileAsync(string key, Stream content, string contentType, CancellationToken cancellationToken = default);
    Task<(string PreSignedUrl, string ObjectKey)> GetPreSignedUrlAsync(string originalFileName, TimeSpan expiry, string contentType);
    Task DeleteFileAsync(string key, CancellationToken cancellationToken = default);
    Task<string> DownloadFileAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}