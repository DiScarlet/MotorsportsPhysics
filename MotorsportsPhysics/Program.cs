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
    options.UseSqlServer(cs);
});

builder.Services.AddSingleton<PasswordSecurityService>();
builder.Services.AddSingleton<LeaderboardService>();


var app = builder.Build();

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
