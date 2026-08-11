using SistemaDeCompras.Models.Enums;

namespace SistemaDeCompras.Models.Entities;

public class AsientoContable
{
    public int Id { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    
    public int CuentaDebitoId { get; set; }
    public int CuentaCreditoId { get; set; }

    public DateTime FechaAsiento { get; set; }
    public decimal MontoAsiento { get; set; }
    public EstadoAsientoContable Estado { get; set; } = EstadoAsientoContable.Pendiente;

    public int? OrdenCompraNumero { get; set; }
    public OrdenCompra? OrdenCompra { get; set; }

    public DateTime? FechaEnvio { get; set; }
    public string? MensajeError { get; set; }
    public int? Asiento { get; set; }
}
