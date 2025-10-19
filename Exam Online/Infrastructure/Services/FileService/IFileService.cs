namespace Exam_Online_API.Infrastructure.Services.FileService
{
    public interface IFileService
    {
        Task<string> UploadFileAsync(IFormFile file, string folder, CancellationToken cancellationToken = default);
        void DeleteFileAsync(string filePath, CancellationToken cancellationToken = default);
        void DeleteFileAsync(List<string> filePaths, CancellationToken cancellationToken = default);
        string GetFileUrl(string filePath);
    }
}
