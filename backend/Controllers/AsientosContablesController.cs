using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCompras.Data;
using SistemaDeCompras.DTOs;
using SistemaDeCompras.Models.Entities;
using SistemaDeCompras.Models.Enums;
using SistemaDeCompras.Services;

namespace SistemaDeCompras.Controllers;

[ApiController]
[Route("api/asientos-contables")]
public class AsientosContablesController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IContabilidadClient _contabilidadClient;

    public AsientosContablesController(AppDbContext context, IContabilidadClient contabilidadClient)
    {
        _context = context;
        _contabilidadClient = contabilidadClient;
    }

    private static AsientoContableDto ToDto(AsientoContable a) => new(
        a.Id, a.Descripcion, a.CuentaDebitoId, a.CuentaCreditoId,
        a.FechaAsiento, a.MontoAsiento, a.Estado, a.OrdenCompraNumero, a.FechaEnvio, a.MensajeError);

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AsientoContableDto>>> GetAll(
        [FromQuery] EstadoAsientoContable? estado, [FromQuery] int? ordenCompraNumero)
    {
        var query = _context.AsientosContables.AsNoTracking().AsQueryable();
        if (estado.HasValue)
            query = query.Where(a => a.Estado == estado.Value);
        if (ordenCompraNumero.HasValue)
            query = query.Where(a => a.OrdenCompraNumero == ordenCompraNumero.Value);

        var result = await query.OrderByDescending(a => a.Id).Select(a => ToDto(a)).ToListAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AsientoContableDto>> GetById(int id)
    {
        var asiento = await _context.AsientosContables.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
        return asiento is null ? NotFound() : Ok(ToDto(asiento));
    }

    [HttpPost("{id:int}/reenviar")]
    public async Task<ActionResult<AsientoContableDto>> Reenviar(int id)
    {
        var asiento = await _context.AsientosContables.FirstOrDefaultAsync(a => a.Id == id);
        if (asiento is null) return NotFound();

        var (success, error) = await _contabilidadClient.EnviarAsientoAsync(asiento);
        asiento.Estado = success ? EstadoAsientoContable.Enviado : EstadoAsientoContable.Error;
        asiento.FechaEnvio = DateTime.UtcNow;
        asiento.MensajeError = error;
        await _context.SaveChangesAsync();

        return Ok(ToDto(asiento));
    }
}
