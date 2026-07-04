using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Options;
using Touir.ExpensesManager.Expenses.Infrastructure.Options;
using Touir.ExpensesManager.Expenses.Services.Contracts;

namespace Touir.ExpensesManager.Expenses.Services
{
    /// <summary>
    /// Talks to an S3-compatible object store (currently backed by MinIO, per <see cref="ReceiptStorageOptions"/>)
    /// via the AWS SDK's generic S3 client.
    /// </summary>
    public class S3ReceiptStorageService : IReceiptStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly SemaphoreSlim _bucketLock = new(1, 1);
        private bool _bucketEnsured;

        public S3ReceiptStorageService(IAmazonS3 s3Client, IOptions<ReceiptStorageOptions> options)
        {
            _s3Client = s3Client;
            _bucketName = options.Value.BucketName;
        }

        public async Task<string> UploadAsync(Stream content, string contentType, long expenseId, string extension, CancellationToken cancellationToken = default)
        {
            await EnsureBucketAsync(cancellationToken);

            var storageKey = $"receipts/{expenseId}/{Guid.NewGuid()}{extension}";

            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = storageKey,
                InputStream = content,
                ContentType = contentType,
                AutoCloseStream = false,
            };

            await _s3Client.PutObjectAsync(request, cancellationToken);

            return storageKey;
        }

        public async Task<(Stream Stream, string ContentType)> GetStreamAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            using var response = await _s3Client.GetObjectAsync(_bucketName, storageKey, cancellationToken);

            var memoryStream = new MemoryStream();
            await response.ResponseStream.CopyToAsync(memoryStream, cancellationToken);
            memoryStream.Position = 0;

            return (memoryStream, response.Headers.ContentType ?? "application/octet-stream");
        }

        public async Task DeleteAsync(string storageKey, CancellationToken cancellationToken = default)
        {
            try
            {
                await _s3Client.DeleteObjectAsync(_bucketName, storageKey, cancellationToken);
            }
            catch (Exception)
            {
                // Idempotent by design: a missing object (or unreachable storage on cleanup) should not fail the caller.
            }
        }

        private async Task EnsureBucketAsync(CancellationToken cancellationToken)
        {
            if (_bucketEnsured)
                return;

            await _bucketLock.WaitAsync(cancellationToken);
            try
            {
                if (_bucketEnsured)
                    return;

                var exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!exists)
                {
                    await _s3Client.PutBucketAsync(new PutBucketRequest { BucketName = _bucketName }, cancellationToken);
                }

                _bucketEnsured = true;
            }
            finally
            {
                _bucketLock.Release();
            }
        }
    }
}
