namespace SistemaDeCompras.Services;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SistemaDeCompras";
    public string Audience { get; set; } = "SistemaDeComprasClient";
    public int ExpirationMinutes { get; set; } = 120;
}
