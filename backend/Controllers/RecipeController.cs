using Microsoft.AspNetCore.Mvc;
using RecipeAPI.Models;
using RecipeAPI.Services;

namespace RecipeAPI.Controllers
{
    [ApiController]
    [Route("api")]
    public class RecipeController : ControllerBase
    {
        private readonly IRecipeService _recipeService;
        private readonly ILeaderboardService _leaderboardService;
        private readonly ILogger<RecipeController> _logger;

        public RecipeController(IRecipeService recipeService, 
                              ILeaderboardService leaderboardService,
                              ILogger<RecipeController> logger)
        {
            _recipeService = recipeService;
            _leaderboardService = leaderboardService;
            _logger = logger;
        }

        [HttpPost("validaterecipe")]
        public async Task<IActionResult> ValidateRecipe([FromForm] string username, [FromForm] IFormFile recipeFile)
        {
            try
            {
                _logger.LogInformation("Recipe validation request received for user: {Username}", username);
                
                var result = await _recipeService.ValidateAndUploadRecipeAsync(username, recipeFile);
                
                if (result.Success)
                {
                    _logger.LogInformation("Recipe successfully uploaded for {Username} with score {Score}", 
                                         username, result.Data?.Score);
                    return Ok(result);
                }
                
                _logger.LogWarning("Recipe validation failed for {Username}: {Message}", username, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing recipe validation for user {Username}", username);
                return StatusCode(500, new RecipeUploadResponse
                {
                    Success = false,
                    Message = "An error occurred while processing your recipe"
                });
            }
        }

        [HttpGet("leaderboard")]
        public async Task<IActionResult> GetLeaderboard()
        {
            try
            {
                _logger.LogInformation("Leaderboard request received");
                
                var result = await _leaderboardService.GetLeaderboardAsync();
                
                if (result.Success)
                {
                    return Ok(result);
                }
                
                _logger.LogWarning("Failed to retrieve leaderboard: {Message}", result.Message);
                return StatusCode(500, result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard");
                return StatusCode(500, new LeaderboardResponse
                {
                    Success = false,
                    Message = "Failed to retrieve leaderboard"
                });
            }
        }

        [HttpPost("adminleaderboard")]
        public async Task<IActionResult> UpdateLeaderboard([FromBody] AdminLeaderboardRequest request)
        {
            try
            {
                _logger.LogInformation("Admin leaderboard update request received for user: {Username}", request.Username);
                
                var result = await _leaderboardService.UpdateLeaderboardAsync(request);
                
                if (result.Success)
                {
                    _logger.LogInformation("Leaderboard successfully updated for {Username}", request.Username);
                    return Ok(result);
                }
                
                _logger.LogWarning("Admin leaderboard update failed for {Username}: {Message}", 
                                 request.Username, result.Message);
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing admin leaderboard update for user {Username}", request.Username);
                return StatusCode(500, new AdminLeaderboardResponse
                {
                    Success = false,
                    Message = "An error occurred while updating leaderboard"
                });
            }
        }
    }
}