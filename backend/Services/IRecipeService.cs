using RecipeAPI.Models;

namespace RecipeAPI.Services
{
    public interface IRecipeService
    {
        Task<RecipeUploadResponse> ValidateAndUploadRecipeAsync(string username, IFormFile recipeFile);
    }
}