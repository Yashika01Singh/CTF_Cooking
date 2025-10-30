namespace RecipeAPI.Models
{
    public class LeaderboardUser
    {
        public string Username { get; set; } = string.Empty;
        public int Score { get; set; }
        public int Rank { get; set; }
        public bool IsNew { get; set; }
    }

    public class LeaderboardResponse
    {
        public bool Success { get; set; }
        public List<LeaderboardUser> Data { get; set; } = new();
        public string Message { get; set; } = string.Empty;
    }

    public class AdminLeaderboardRequest
    {
        public string Username { get; set; } = string.Empty;
        public int Points { get; set; }
        public string AdminPassword { get; set; } = string.Empty;
    }

    public class AdminLeaderboardResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public AdminLeaderboardData? Data { get; set; }
    }

    public class AdminLeaderboardData
    {
        public List<LeaderboardUser> Leaderboard { get; set; } = new();
        public string? Flag { get; set; }
    }
}