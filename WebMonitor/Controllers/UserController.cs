using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebMonitor.Model.Db;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly JwtOptions _jwtOptions;
    private readonly WebMonitorContext _db;
    private readonly SupportedFeatures _supportedFeatures;

    public UserController(IServiceProvider serviceProvider, ILogger<UserController> logger)
    {
        _logger = logger;
        _jwtOptions = serviceProvider.GetRequiredService<JwtOptions>();
        _db = serviceProvider.GetRequiredService<WebMonitorContext>();
        _supportedFeatures = serviceProvider.GetRequiredService<SupportedFeatures>();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] FormUser formUser)
    {
        if (await _db.Users.AnyAsync(u => u.Username == formUser.Username))
            return BadRequest("Username already exists");

        var passwordHasher = new PasswordHasher<FormUser>();
        var password = passwordHasher.HashPassword(formUser, formUser.Password);
        // First created user is automatically promoted to admin
        var isAdmin = !_db.Users.Any();
        var user = new User
        {
            Username = formUser.Username,
            DisplayName = formUser.DisplayName,
            Password = password,
            IsAdmin = isAdmin
        };

        if (isAdmin) 
            user.AllowedFeatures = _supportedFeatures;

        var createdUser = await _db.Users.AddAsync(user);
        await _db.SaveChangesAsync();

        var stringToken = GetToken(createdUser.Entity);
        _logger.LogInformation("User {Username} registered", createdUser.Entity.Username);
        
        return Ok(new
        {
            token = stringToken, 
            user = new
            {
                user.Username,
                user.DisplayName,
                user.IsAdmin,
                user.AllowedFeatures
            }
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult> Login([FromBody] LoginFormUser formUser)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == formUser.Username);
        if (user is null)
            return BadRequest("Invalid username or password");

        var passwordHasher = new PasswordHasher<LoginFormUser>();
        var result = passwordHasher.VerifyHashedPassword(formUser, user.Password, formUser.Password);
        if (result == PasswordVerificationResult.Failed)
            return BadRequest("Invalid username or password");

        var stringToken = GetToken(user);
        _logger.LogInformation("User {Username} logged in", user.Username);
        
        return Ok(new
        {
            token = stringToken,
            user = new
            {
                user.Username,
                user.DisplayName,
                user.IsAdmin,
                user.AllowedFeatures
            }
        });
    }

    [HttpGet("me"), Authorize]
    public async Task<ActionResult> Me()
    {
        if (User.Identity is null)
            return BadRequest("User not logged in");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
        if (user is null)
            return BadRequest("Invalid username or password");

        return Ok(new { user.Username, user.DisplayName, user.IsAdmin, user.AllowedFeatures });
    }

    [HttpPost("promoteToAdmin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> PromoteToAdmin([FromBody] string username)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
            return BadRequest("Invalid username");

        user.IsAdmin = true;
        // Allow the user to use all features
        user.AllowedFeatures = _supportedFeatures;
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("User {Username} promoted to admin", user.Username);
        
        return Ok($"User {username} promoted to admin");
    }

    [HttpGet("listUsers"), Authorize(Roles = "Admin")]
    public ActionResult ListUsers()
    {
        var users = _db.Users.Join(_db.SupportedFeatures, u => u.AllowedFeaturesId, sf => sf.Id, (u, sf) => new
        {
            u.Username,
            u.DisplayName,
            u.IsAdmin,
            AllowedFeatures = sf
        });
        return Ok(users);
    }

    [HttpDelete("deleteSelf"), Authorize]
    public async Task<ActionResult<string>> DeleteSelf()
    {
        if (User.Identity is null)
            return BadRequest("User not logged in");

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
        if (user is null)
            return BadRequest("Invalid username");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("User {Username} deleted", user.Username);
        
        return Ok("Account deleted");
    }

    [HttpDelete("deleteUser"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> DeleteUser([FromBody] string username)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
            return BadRequest("Invalid username");

        if (user.IsAdmin)
            return BadRequest("Cannot delete admin user");

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();
        
        _logger.LogInformation("User {Username} deleted", user.Username);
        
        return Ok($"User {username} deleted");
    }

    private string GetToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, "TokenForApi"),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTime.UtcNow.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Username)
        };

        if (user.IsAdmin)
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));

        var token = new JwtSecurityToken(_jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: DateTime.UtcNow.AddDays(365),
            signingCredentials: new SigningCredentials(new SymmetricSecurityKey(_jwtOptions.Key),
                SecurityAlgorithms.HmacSha256)
        );

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(token);
    }

    public record FormUser(string Username, string DisplayName, string Password);

    public record LoginFormUser(string Username, string Password);
}