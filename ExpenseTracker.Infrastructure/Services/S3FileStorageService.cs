using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ExpenseTracker.Infrastructure.Services;

public class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ICurrentUserService _currentUserService;
    private readonly string _bucketName;

    public S3FileStorageService(IAmazonS3 s3Client, ICurrentUserService currentUserService, IOptions<StorageOptions> storageOptions)
    {
        _s3Client = s3Client;
        _currentUserService = currentUserService;
        _bucketName = storageOptions.Value.BucketName;
    }

    public async Task<string> UploadFileAsync(
        string key,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = content,
            ContentType = contentType,
            DisablePayloadSigning = true // wymagane dla MinIO / S3-compatible
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);

        return key;
    }

    public Task<(string PreSignedUrl, string ObjectKey)> GetPreSignedUrlAsync(string originalFileName, TimeSpan expiry, string contentType)
    {
        var extension = Path.GetExtension(originalFileName).ToLowerInvariant();
        var objectKey = $"temp/{Guid.NewGuid()}{extension}";

        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.Add(expiry),
            Protocol = Protocol.HTTP,
            // ContentType = contentType
            Verb = HttpVerb.PUT
        };

        var url = _s3Client.GetPreSignedURL(request);
        return Task.FromResult((url, objectKey));
    }

    public async Task DeleteFileAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        var request = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = key
        };

        await _s3Client.DeleteObjectAsync(request, cancellationToken);
    }

    public Task<string> DownloadFileAsync(
        string key,
        TimeSpan expiry,
        CancellationToken cancellationToken = default)
    {
        var request = new GetPreSignedUrlRequest
        {
            BucketName = _bucketName,
            Key = key,
            Verb = HttpVerb.GET,
            Expires = DateTime.UtcNow.Add(expiry),
            Protocol = Protocol.HTTP
        };

        var preSignedUrl = _s3Client.GetPreSignedURL(request);

        return Task.FromResult(preSignedUrl);
    }
}
