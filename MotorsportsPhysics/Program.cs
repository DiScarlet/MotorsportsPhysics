using Microsoft.EntityFrameworkCore;
using MotorsportsPhysics.Components;
using MotorsportsPhysics.Data;
using MotorsportsPhysics.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// MVC controllers for auth endpoint
builder.Services.AddControllers();

// AuthN/Z
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/login";
        options.LogoutPath = "/logout";
        options.SlidingExpiration = true;
    });
builder.Services.AddAuthorization();
builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<MotorsportsDbContext>(options =>
{
    var envConn = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection");
    var cs = string.IsNullOrWhiteSpace(envConn)
        ? builder.Configuration.GetConnectionString("DefaultConnection")
        : envConn;
    // Enable connection resiliency and sensible timeouts
    options.UseSqlServer(cs, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(maxRetryCount: 5, maxRetryDelay: TimeSpan.FromSeconds(10), errorNumbersToAdd: null);
    });
});

builder.Services.AddSingleton<PasswordSecurityService>();
builder.Services.AddScoped<LeaderboardService>();

var app = builder.Build();

// Validate DB connection at startup (log useful errors for AAD / network issues)
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        var db = scope.ServiceProvider.GetRequiredService<MotorsportsDbContext>();
        // Try open/close to surface connection/authentication errors early
        db.Database.SetCommandTimeout(30);
        db.Database.OpenConnection();
        db.Database.CloseConnection();
        logger.LogInformation("Database connection validated successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to validate database connection. Check connection string, network/firewall and authentication (AAD/SQL auth).\nError: {Message}", ex.Message);
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

// Ensure legacy /favicon.ico requests receive the updated PNG
app.MapGet("/favicon.ico", context =>
{
    context.Response.Redirect("/favicon.png?v=3", permanent: false);
    return Task.CompletedTask;
});

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapControllers();

app.Run();
