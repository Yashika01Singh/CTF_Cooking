using RecipeAPI.Models;
using System.Collections.Concurrent;

namespace RecipeAPI.Services
{
    public class LeaderboardService : ILeaderboardService
    {
        private readonly ConcurrentDictionary<string, int> _leaderboard = new();
        private readonly ILogger<LeaderboardService> _logger;
        // admin password
        private readonly string ADMIN_PASSWORD = Environment.GetEnvironmentVariable("ADMIN_PASSWORD") ?? "default-admin-password";
        private readonly string ADMIN_FLAG = Environment.GetEnvironmentVariable("ADMIN_FLAG") ?? "default-flag";

        public async Task<LeaderboardResponse> GetLeaderboardAsync()
        {
            try
            {
                var sortedUsers = _leaderboard
                    .OrderByDescending(kvp => kvp.Value)
                    .Select((kvp, index) => new LeaderboardUser
                    {
                        Username = kvp.Key,
                        Score = kvp.Value,
                        Rank = index + 1
                    })
                    .ToList();

                return new LeaderboardResponse
                {
                    Success = true,
                    Data = sortedUsers,
                    Message = "Leaderboard retrieved successfully"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving leaderboard");
                return new LeaderboardResponse
                {
                    Success = false,
                    Message = "Failed to retrieve leaderboard"
                };
            }
        }

        public async Task<AdminLeaderboardResponse> UpdateLeaderboardAsync(AdminLeaderboardRequest request)
        {
            try
            {
                // Check admin password (or internal update)
                if (request.AdminPassword != ADMIN_PASSWORD && request.AdminPassword != "internal-update")
                {
                    _logger.LogWarning("Invalid admin password attempt from user {Username}", request.Username);
                    return new AdminLeaderboardResponse
                    {
                        Success = false,
                        Message = "Invalid admin password"
                    };
                }

                // Validate input
                if (string.IsNullOrWhiteSpace(request.Username) || request.Points < 0)
                {
                    return new AdminLeaderboardResponse
                    {
                        Success = false,
                        Message = "Invalid username or points"
                    };
                }

                // Update or add user to leaderboard
                bool isNewUser = !_leaderboard.ContainsKey(request.Username);
                _leaderboard.AddOrUpdate(request.Username, request.Points, (key, oldValue) => 
                {
                    // For internal updates, add to existing score
                    if (request.AdminPassword == "internal-update")
                    {
                        return oldValue + request.Points;
                    }
                    // For admin updates, set exact score
                    return request.Points;
                });

                // Get updated leaderboard
                var leaderboard = GetLeaderboardAsync().Result;
                
                // Mark new entries
                if (isNewUser && leaderboard.Success)
                {
                    var newUser = leaderboard.Data.FirstOrDefault(u => u.Username == request.Username);
                    if (newUser != null)
                    {
                        newUser.IsNew = true;
                    }
                }

                var response = new AdminLeaderboardResponse
                {
                    Success = true,
                    Message = isNewUser ? "User added to leaderboard successfully!" : "Leaderboard updated successfully!",
                    Data = new AdminLeaderboardData
                    {
                        Leaderboard = leaderboard.Data
                    }
                };

                // Award flag for successful admin access (not internal updates)
                if (request.AdminPassword == ADMIN_PASSWORD)
                {
                    response.Data.Flag = ADMIN_FLAG;
                    _logger.LogInformation("Admin flag awarded to user {Username}", request.Username);
                }

                _logger.LogInformation("Leaderboard updated: {Username} = {Points} points", 
                                     request.Username, _leaderboard[request.Username]);

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating leaderboard");
                return new AdminLeaderboardResponse
                {
                    Success = false,
                    Message = "Failed to update leaderboard"
                };
            }
        }
    }
}