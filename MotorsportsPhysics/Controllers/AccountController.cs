using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MotorsportsPhysics.Data;
using MotorsportsPhysics.Services;

namespace MotorsportsPhysics.Controllers;

[ApiController]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly MotorsportsDbContext _context;
    private readonly PasswordSecurityService _passwordSecurity;

    public AccountController(MotorsportsDbContext ctx, PasswordSecurityService ps)
    {
        _context = ctx;
        _passwordSecurity = ps;
    }

    public sealed class LoginVm
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
    }

    [HttpPost("/auth/login")]
    public async Task<IActionResult> Login([FromForm] LoginVm model)
    {
        if (string.IsNullOrWhiteSpace(model.UserName) || string.IsNullOrWhiteSpace(model.Password))
            return BadRequest("Missing credentials");

        var user = _context.Users.FirstOrDefault(u => u.UserName == model.UserName);
        if (user == null) return Unauthorized();

        var ok = await _passwordSecurity.VerifyAsync(model.Password, user.PasswordSalt ?? string.Empty, user.PasswordHashPHC);
        if (!ok) return Unauthorized();

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.GivenName, user.FirstName ?? string.Empty),
            new(ClaimTypes.Surname, user.LastName ?? string.Empty)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        return Redirect("/");
    }
}
