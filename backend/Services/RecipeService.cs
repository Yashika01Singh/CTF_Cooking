using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;
using RecipeAPI.Models;
using System.Text;
using System.Text.Json;

namespace RecipeAPI.Services
{
    public class RecipeService : IRecipeService
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly AzureStorageConfig _config;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<RecipeService> _logger;


        public RecipeService(BlobServiceClient blobServiceClient, 
                           IOptions<AzureStorageConfig> config,
                           ILeaderboardService leaderboardService,
                           ILogger<RecipeService> logger)
        {
            _blobServiceClient = blobServiceClient;
            _config = config.Value;
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        public async Task<RecipeUploadResponse> ValidateAndUploadRecipeAsync(string username, IFormFile recipeFile)
        {
            try
            {
                // Validate input
                if (string.IsNullOrWhiteSpace(username))
                {
                    return new RecipeUploadResponse
                    {
                        Success = false,
                        Message = "Username is required"
                    };
                }

                if (recipeFile == null || recipeFile.Length == 0)
                {
                    return new RecipeUploadResponse
                    {
                        Success = false,
                        Message = "Recipe file is required"
                    };
                }

                // Validate file size (10MB max)
                if (recipeFile.Length > 10 * 1024 * 1024)
                {
                    return new RecipeUploadResponse
                    {
                        Success = false,
                        Message = "File size exceeds 10MB limit"
                    };
                }

                // Read file content for scoring
                string content;
                using (var reader = new StreamReader(recipeFile.OpenReadStream()))
                {
                    content = await reader.ReadToEndAsync();
                }

                // Calculate score based on secret ingredients
                int score = CalculateRecipeScore(content);

                // Upload to Azure Storage
                var storageUrl = await UploadToAzureStorageAsync(username, recipeFile, content);

                // Update leaderboard if score > 0
                if (score > 0)
                {
                    var adminRequest = new AdminLeaderboardRequest
                    {
                        Username = username,
                        Points = score,
                        AdminPassword = "internal-update" // Internal flag
                    };
                    await _leaderboardService.UpdateLeaderboardAsync(adminRequest);
                }

                return new RecipeUploadResponse
                {
                    Success = true,
                    Message = "Recipe uploaded and validated successfully!",
                    Data = new RecipeUploadData
                    {
                        Score = score,
                        StorageUrl = storageUrl,
                        FileName = recipeFile.FileName
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading recipe for user {Username}", username);
                return new RecipeUploadResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your recipe"
                };
            }
        }

        private int CalculateRecipeScore(string content)
        {
            int totalScore = 0;
            
            foreach (var ingredient in _secretIngredients)
            {
                if (content.Contains(ingredient.Key, StringComparison.OrdinalIgnoreCase))
                {
                    totalScore += ingredient.Value;
                    _logger.LogInformation("Found secret ingredient: {Ingredient} (+{Score} points)", 
                                         ingredient.Key, ingredient.Value);
                }
            }

            // Bonus for multiple ingredients
            if (totalScore > 200)
            {
                totalScore += 50; // Combo bonus
            }

            return totalScore;
        }

        private async Task<string> UploadToAzureStorageAsync(string username, IFormFile file, string content)
        {
            try
            {
                // Create container if it doesn't exist
                var containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);
                await containerClient.CreateIfNotExistsAsync();

                // Generate unique blob name
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
                var fileExtension = Path.GetExtension(file.FileName);
                var blobName = $"{username}_{timestamp}_{file.FileName}";

                // Upload file
                var blobClient = containerClient.GetBlobClient(blobName);
                
                using (var stream = file.OpenReadStream())
                {
                    await blobClient.UploadAsync(stream, overwrite: true);
                }

                // Set metadata
                var metadata = new Dictionary<string, string>
                {
                    { "username", username },
                    { "uploadTime", DateTime.UtcNow.ToString("O") },
                    { "originalFileName", file.FileName },
                    { "fileSize", file.Length.ToString() }
                };
                await blobClient.SetMetadataAsync(metadata);

                // Create special entry for top-chef user (CTF easter egg)
                if (username.Equals("top-chef", StringComparison.OrdinalIgnoreCase))
                {
                    await CreateTopChefRecipe(containerClient);
                }

                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading to Azure Storage");
                throw new InvalidOperationException("Failed to upload file to storage", ex);
            }
        }
}