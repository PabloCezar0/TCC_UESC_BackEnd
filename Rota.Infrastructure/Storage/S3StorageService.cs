using Amazon.S3;
using Amazon.S3.Model;
using Rota.Application.Common.Storage;
using Microsoft.Extensions.Configuration;

namespace Rota.Infrastructure.Storage;

public sealed class S3StorageService : IStorageService
{
    private readonly IAmazonS3 _s3;
    private readonly string    _bucket;

    public S3StorageService(IAmazonS3 s3, IConfiguration cfg)
    {
        _s3    = s3;
        _bucket = cfg["AWS_BUCKET"];      
    }

    public async Task<string> UploadAsync(Stream content, string key,
                                          string contentType,
                                          CancellationToken ct = default)
    {
        var req = new PutObjectRequest
        {
            InputStream = content,
            BucketName  = _bucket,
            Key         = key,
            ContentType = contentType
        };
        await _s3.PutObjectAsync(req, ct);
        return $"https://{_bucket}.s3.{_s3.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
    }

    public async Task<Stream> DownloadAsync(string key, CancellationToken ct = default)
    {
        var res = await _s3.GetObjectAsync(_bucket, key, ct);
        return res.ResponseStream;
    }

    public Task DeleteAsync(string key, CancellationToken ct = default) =>
        _s3.DeleteObjectAsync(_bucket, key, ct);
}

