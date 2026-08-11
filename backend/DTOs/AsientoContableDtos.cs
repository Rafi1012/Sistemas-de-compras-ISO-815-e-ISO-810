using SistemaDeCompras.Models.Enums;

namespace SistemaDeCompras.DTOs;

public record AsientoContableDto(
    int Id,
    string Descripcion,
    int CuentaDebitoId,
    int CuentaCreditoId,
    DateTime FechaAsiento,
    decimal MontoAsiento,
    EstadoAsientoContable Estado,
    int? OrdenCompraNumero,
    DateTime? FechaEnvio,
    string? MensajeError,
    int? Asiento);
