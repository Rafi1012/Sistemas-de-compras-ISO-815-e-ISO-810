using System.Text.Json.Serialization;

namespace SistemaDeCompras.Services;

public class AsientoContableWsResponse
{
    [JsonPropertyName("numeroAsiento")]
    public int? NumeroAsiento { get; set; }
}
