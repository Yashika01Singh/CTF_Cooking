using RecipeAPI.Models;

namespace RecipeAPI.Services
{
    public interface ILeaderboardService
    {
        Task<LeaderboardResponse> GetLeaderboardAsync();
        Task<AdminLeaderboardResponse> UpdateLeaderboardAsync(AdminLeaderboardRequest request);
    }
}