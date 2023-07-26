namespace WebMonitor;

public record JwtOptions(
    string Issuer,
    string Audience,
    byte[] Key
);
