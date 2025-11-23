using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MotorsportsPhysics.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;

namespace MotorsportsPhysics.Services;

public class LeaderboardService
{
    private readonly ILogger<LeaderboardService> _logger;

    public LeaderboardService(ILogger<LeaderboardService> logger)
    {
        _logger = logger;
    }

    public async Task<bool> SaveAsync(IHttpContextAccessor httpAccessor, MotorsportsDbContext db,
        string difficulty, string levelName, int lapTimeMs, int? finishedPosition)
    {
        var user = httpAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return false;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrWhiteSpace(userName)) return false;

        try
        {
            // Compute leaderboard rank among existing entries for same difficulty/level by best time
            var betterCount = await db.Leaderboards
                .Where(l => l.Difficulty == difficulty && l.LevelName == levelName && l.LapTimeMs != null && l.LapTimeMs < lapTimeMs)
                .CountAsync();
            var position = betterCount + 1;

            // Insert row into table (entity is keyless, so use raw SQL)
            var sql = "INSERT INTO dbo.Leaderboard (Position, UserName, Difficulty, LevelName, LapTimeMs, FinishedPosition) VALUES (@p0, @p1, @p2, @p3, @p4, @p5)";
            var affected = await db.Database.ExecuteSqlRawAsync(sql, position, userName, difficulty, levelName, lapTimeMs, finishedPosition);
            return affected > 0;
        }
        catch (SqlException sqlEx)
        {
            _logger.LogError(sqlEx, "SQL error saving leaderboard entry: {Message}", sqlEx.Message);
            return false;
        }
        catch (DbUpdateException dbEx)
        {
            _logger.LogError(dbEx, "EF Core update error saving leaderboard entry: {Message}", dbEx.Message);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error saving leaderboard entry: {Message}", ex.Message);
            return false;
        }
    }
}
