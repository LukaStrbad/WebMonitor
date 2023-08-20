using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;

namespace WebMonitor.Utility;

public class AuthUtility
{
    public JwtOptions JwtOptions { get; init; }
    
    
    public AuthUtility(JwtOptions jwtOptions)
    {
        JwtOptions = jwtOptions;
    }

    public ClaimsPrincipal ValidateToken(string accessToken)
    {
        // Authenticate user using the access token
        var tokenHandler = new JwtSecurityTokenHandler();
        var validateToken = tokenHandler.ValidateToken(accessToken, new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = JwtOptions.Issuer,
            ValidateAudience = true,
            ValidAudience = JwtOptions.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(JwtOptions.Key),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out _);

        return validateToken;
    }
}