using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using MotorsportsPhysics.Data;

namespace MotorsportsPhysics.Services;

public class LeaderboardService
{
    public async Task<bool> SaveAsync(IHttpContextAccessor httpAccessor, MotorsportsDbContext db,
        string difficulty, string levelName, int lapTimeMs, int? finishedPosition)
    {
        var user = httpAccessor.HttpContext?.User;
        if (user?.Identity?.IsAuthenticated != true) return false;
        var userName = user.FindFirst(ClaimTypes.Name)?.Value;
        if (string.IsNullOrWhiteSpace(userName)) return false;

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
}
