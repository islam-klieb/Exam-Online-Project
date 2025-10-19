namespace Exam_Online_API.Shared.Validators
{
    public static class ImageValidationHelper
    {
        private static readonly string[] AllowedExtensions = { ".jpg", ".jpeg", ".png" };

        public static bool BeValidImageFile(IFormFile? file)
        {
            if (file == null) return false;

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            return AllowedExtensions.Contains(extension);
        }

        public static bool BeValidFileSize(IFormFile? file, int maxSizeMB = 2)
        {
            if (file == null) return false;

            return file.Length <= maxSizeMB * 1024 * 1024;
        }
    }
}
