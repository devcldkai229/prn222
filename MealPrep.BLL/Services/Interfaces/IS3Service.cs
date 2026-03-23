namespace MealPrep.BLL.Services.Interfaces
{
    /// <summary>
    /// Service for uploading files to AWS S3
    /// </summary>
    public interface IS3Service
    {
        /// <summary>
        /// Upload a file to S3 and return the S3 key
        /// </summary>
        /// <param name="fileStream">File stream to upload</param>
        /// <param name="fileName">Original file name</param>
        /// <param name="folder">Folder path in S3 (e.g., "meals/", "delivery-proofs/")</param>
        /// <param name="contentType">MIME type of the file (e.g., "image/jpeg")</param>
        /// <returns>S3 key (path) of the uploaded file</returns>
        Task<string> UploadFileAsync(Stream fileStream, string fileName, string folder, string contentType);

        /// <summary>
        /// Get a presigned URL for accessing the file (valid for 1 hour by default)
        /// </summary>
        /// <param name="s3Key">S3 key of the file</param>
        /// <param name="expirationHours">URL expiration time in hours (default: 1)</param>
        /// <returns>Presigned URL</returns>
        string GetPresignedUrl(string s3Key, int expirationHours = 1);

        /// <summary>
        /// Delete a file from S3
        /// </summary>
        /// <param name="s3Key">S3 key of the file to delete</param>
        Task DeleteFileAsync(string s3Key);

        /// <summary>
        /// Check if a file exists in S3
        /// </summary>
        /// <param name="s3Key">S3 key of the file</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExistsAsync(string s3Key);

        /// <summary>
        /// Get public URL for S3 object (if bucket is configured for public access)
        /// </summary>
        /// <param name="s3Key">S3 key of the file</param>
        /// <returns>Public URL</returns>
        string GetPublicUrl(string s3Key);

        /// <summary>
        /// Resolve meal images (S3 keys or legacy URLs) to displayable URLs.
        /// Keys are converted to presigned URLs; full URLs are returned as-is.
        /// </summary>
        string[] ResolveMealImageUrls(string[]? imagesOrKeys);
    }
}
