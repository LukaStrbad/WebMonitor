using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using WebMonitor.Model;
using WebMonitor.Model.Db;

namespace WebMonitor.Controllers;

[ApiController]
[Route("[controller]")]
[Produces("application/json")]
public class UserController : ControllerBase
{
    private readonly ILogger<UserController> _logger;
    private readonly JwtOptions _jwtOptions;
    private readonly SupportedFeatures _supportedFeatures;

    public UserController(IServiceProvider serviceProvider, ILogger<UserController> logger)
    {
        _logger = logger;
        _jwtOptions = serviceProvider.GetRequiredService<JwtOptions>();
        _supportedFeatures = serviceProvider.GetRequiredService<SupportedFeatures>();
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] FormUser formUser)
    {
        await using var db = new WebMonitorContext();

        if (await db.Users.AnyAsync(u => u.Username == formUser.Username))
            return BadRequest("Username already exists");

        var passwordHasher = new PasswordHasher<FormUser>();
        var password = passwordHasher.HashPassword(formUser, formUser.Password);
        // First created user is automatically promoted to admin
        var isAdmin = !db.Users.Any();
        var user = new User
        {
            Username = formUser.Username,
            DisplayName = formUser.DisplayName,
            Password = password,
            IsAdmin = isAdmin
        };

        if (isAdmin)
            user.AllowedFeatures = AllowedFeatures.All;

        var createdUser = await db.Users.AddAsync(user);
        await db.SaveChangesAsync();

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
        await using var db = new WebMonitorContext();

        var users = await db.Users
                    .Where(u => u.Username == formUser.Username)
                    .Include(u => u.AllowedFeatures)
                    .ToListAsync();
        var user = users.FirstOrDefault();
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
        await using var db = new WebMonitorContext();

        if (User.Identity is null)
            return BadRequest("User not logged in");

        var users = await db.Users
            .Where(u => u.Username == User.Identity.Name)
            .Include(u => u.AllowedFeatures)
            .ToListAsync();
        if (users.Count != 1)
            return BadRequest("Invalid username or password");

        var user = users[0];

        return Ok(new { user.Username, user.DisplayName, user.IsAdmin, user.AllowedFeatures });
    }

    [HttpPost("promoteToAdmin"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> PromoteToAdmin([FromBody] UsernameRequest request)
    {
        await using var db = new WebMonitorContext();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
            return BadRequest("Invalid username");

        user.IsAdmin = true;
        // Allow the user to use all features
        user.AllowedFeatures = AllowedFeatures.All;
        await db.SaveChangesAsync();

        _logger.LogInformation("User {Username} promoted to admin", user.Username);

        return Ok($"User {request.Username} promoted to admin");
    }

    [HttpPost("leaveAdminRole"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> LeaveAdminRole()
    {
        await using var db = new WebMonitorContext();

        if (User.Identity is null)
            return BadRequest("User not logged in");

        var adminCount = await db.Users.CountAsync(u => u.IsAdmin);
        if (adminCount == 1)
            return BadRequest("Cannot leave admin role when you are the only admin");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == User.Identity.Name);
        if (user is null)
            return BadRequest("Invalid username");

        user.IsAdmin = false;
        await db.SaveChangesAsync();

        _logger.LogInformation("User {Username} demoted from admin", user.Username);

        return Ok($"User {user.Username} demoted from admin");
    }

    [HttpGet("listUsers"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<User>>> ListUsers()
    {
        await using var db = new WebMonitorContext();

        var users = await db.Users
            .Include(u => u.AllowedFeatures)
            .ToListAsync();
        return Ok(users);
    }

    [HttpDelete("deleteUser"), Authorize(Roles = "Admin")]
    public async Task<ActionResult<string>> DeleteUser([FromQuery] string username)
    {
        await using var db = new WebMonitorContext();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user is null)
            return BadRequest("Invalid username");

        // Stop the admin user from deleting another admin user but allow self-deletion
        if (user.IsAdmin && User.Identity?.Name != username)
            return BadRequest("Cannot delete admin user");

        db.Users.Remove(user);
        await db.SaveChangesAsync();

        _logger.LogInformation("User {Username} deleted", user.Username);

        return Ok($"User {username} deleted");
    }

    [HttpPost("changeAllowedFeatures"), Authorize(Roles = "Admin")]
    public async Task<ActionResult> ChangeAllowedFeatures([FromBody] ChangeAllowedFeaturesForm request)
    {
        await using var db = new WebMonitorContext();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
        if (user is null)
            return BadRequest("Invalid username");

        if (user.IsAdmin)
            return BadRequest("Cannot change allowed features for admin user");

        var features = await db.AllowedFeatures.FirstOrDefaultAsync(af => af.Id == user.AllowedFeaturesId);
        if (features is null)
        {
            // IF the user has no allowed features, create a new AllowedFeatures object
            user.AllowedFeatures = request.AllowedFeatures;
            await db.SaveChangesAsync();
            return Ok();
        }

        // Update the AllowedFeatures object
        foreach (var feature in request.AllowedFeatures.GetType().GetProperties())
        {
            if (feature.PropertyType != typeof(bool))
                continue;

            var featureValue = (bool?)feature.GetValue(request.AllowedFeatures) ?? false;

            feature.SetValue(features, featureValue);
        }

        await db.SaveChangesAsync();
        return Ok();
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

    public record ChangeAllowedFeaturesForm(string Username, AllowedFeatures AllowedFeatures);

    public record UsernameRequest(string Username);
}
