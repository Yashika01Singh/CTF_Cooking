namespace RecipeAPI.Models
{
    public class RecipeUploadRequest
    {
        public string Username { get; set; } = string.Empty;
        public IFormFile RecipeFile { get; set; } = null!;
    }

    public class RecipeUploadResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public RecipeUploadData? Data { get; set; }
    }

    public class RecipeUploadData
    {
        public int Score { get; set; }
        public string StorageUrl { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
    }
}