using System.Text.Json.Serialization;

namespace SistemaDeCompras.Services;

public class AsientoContableWsPayload
{
    [JsonPropertyName("auxiliarId")]
    public int AuxiliarId { get; set; } = 5;

    [JsonPropertyName("cuentaDebitoId")]
    public int CuentaDebitoId { get; set; }

    [JsonPropertyName("cuentaCreditoId")]
    public int CuentaCreditoId { get; set; }

    [JsonPropertyName("descripcion")]
    public string Descripcion { get; set; } = string.Empty;

    [JsonPropertyName("monto")]
    public decimal Monto { get; set; }
}
