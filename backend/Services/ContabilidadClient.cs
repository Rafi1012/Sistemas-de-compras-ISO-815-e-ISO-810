using Microsoft.Extensions.Options;
using SistemaDeCompras.Models.Entities;
using SistemaDeCompras.Models.Enums;

namespace SistemaDeCompras.Services;

public class ContabilidadClient : IContabilidadClient
{
    private readonly HttpClient _httpClient;
    private readonly ContabilidadOptions _options;
    private readonly ILogger<ContabilidadClient> _logger;

    public ContabilidadClient(HttpClient httpClient, IOptions<ContabilidadOptions> options, ILogger<ContabilidadClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<(bool Success, string? Error)> EnviarAsientoAsync(AsientoContable asiento, CancellationToken ct = default)
    {
        var payload = new AsientoContableWsPayload
        {
            AuxiliarId = 5,
            CuentaDebitoId = asiento.CuentaDebitoId,
            CuentaCreditoId = asiento.CuentaCreditoId,
            Descripcion = asiento.Descripcion,
            Monto = asiento.MontoAsiento
        };

        try
        {
            using var response = await _httpClient.PostAsJsonAsync(_options.AsientosEndpoint, payload, ct);
            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            var body = await response.Content.ReadAsStringAsync(ct);
            _logger.LogWarning("El WS de Contabilidad respondio {StatusCode} para el asiento {Id}: {Body}",
                response.StatusCode, asiento.Id, body);
            return (false, $"HTTP {(int)response.StatusCode}: {body}");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning(ex, "No se pudo contactar el WS de Contabilidad para el asiento {Id}", asiento.Id);
            return (false, ex.Message);
        }
    }
}
