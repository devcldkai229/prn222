using System.Text;
using Amazon.S3;
using Amazon.S3.Model;
using MealPrep.BLL.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MealPrep.BLL.Services
{
    public class S3Service : IS3Service
    {
        private readonly IAmazonS3 _s3Client;
        private readonly string _bucketName;
        private readonly string _region;
        private readonly ILogger<S3Service> _logger;

        public S3Service(
            IAmazonS3 s3Client,
            IConfiguration configuration,
            ILogger<S3Service> logger
        )
        {
            _s3Client = s3Client;
            _bucketName = configuration["AwsS3:BucketName"] ?? "mealprep-storage-data";
            _region = configuration["AwsS3:Region"] ?? "ap-northeast-1";
            _logger = logger;
        }

        public async Task<string> UploadFileAsync(
            Stream fileStream,
            string fileName,
            string folder,
            string contentType
        )
        {
            try
            {
                // Generate unique file name to avoid conflicts
                var fileExtension = Path.GetExtension(fileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var s3Key = $"{folder.TrimEnd('/')}/{uniqueFileName}";

                var request = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    InputStream = fileStream,
                    ContentType = contentType,
                    // Note: CannedACL is deprecated. Use bucket policy for public access instead.
                };

                var response = await _s3Client.PutObjectAsync(request);

                if (response.HttpStatusCode == System.Net.HttpStatusCode.OK)
                {
                    _logger.LogInformation("Successfully uploaded file to S3: {S3Key}", s3Key);
                    return s3Key;
                }
                else
                {
                    throw new Exception(
                        $"Failed to upload file to S3. Status: {response.HttpStatusCode}"
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading file to S3: {FileName}", fileName);
                throw;
            }
        }

        public string GetPresignedUrl(string s3Key, int expirationHours = 1)
        {
            try
            {
                var request = new GetPreSignedUrlRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                    Verb = HttpVerb.GET,
                    Expires = DateTime.UtcNow.AddHours(expirationHours),
                };

                var url = _s3Client.GetPreSignedURL(request);
                return url;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating presigned URL for S3 key: {S3Key}", s3Key);
                throw;
            }
        }

        /// <summary>
        /// Get public URL for S3 object (if bucket is configured for public access)
        /// Format: https://{bucketName}.s3.{region}.amazonaws.com/{key}
        /// </summary>
        public string GetPublicUrl(string s3Key)
        {
            return $"https://{_bucketName}.s3.{_region}.amazonaws.com/{s3Key}";
        }

        public async Task DeleteFileAsync(string s3Key)
        {
            try
            {
                var request = new DeleteObjectRequest { BucketName = _bucketName, Key = s3Key };

                await _s3Client.DeleteObjectAsync(request);
                _logger.LogInformation("Successfully deleted file from S3: {S3Key}", s3Key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file from S3: {S3Key}", s3Key);
                throw;
            }
        }

        public async Task<bool> FileExistsAsync(string s3Key)
        {
            try
            {
                var request = new GetObjectMetadataRequest
                {
                    BucketName = _bucketName,
                    Key = s3Key,
                };

                await _s3Client.GetObjectMetadataAsync(request);
                return true;
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking file existence in S3: {S3Key}", s3Key);
                throw;
            }
        }

        public string[] ResolveMealImageUrls(string[]? imagesOrKeys)
        {
            if (imagesOrKeys == null || imagesOrKeys.Length == 0)
                return [];
            return imagesOrKeys.Select(x =>
            {
                if (string.IsNullOrEmpty(x)) return "";
                if (x.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || x.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                    return x;
                return GetPresignedUrl(x, 24);
            }).ToArray();
        }
    }
}
