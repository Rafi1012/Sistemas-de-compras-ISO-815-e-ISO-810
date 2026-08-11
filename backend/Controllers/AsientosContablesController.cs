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
    private readonly IBackgroundTaskQueue _backgroundTaskQueue;
    private readonly ILogger<AsientosContablesController> _logger;

    public AsientosContablesController(AppDbContext context, IBackgroundTaskQueue backgroundTaskQueue, ILogger<AsientosContablesController> logger)
    {
        _context = context;
        _backgroundTaskQueue = backgroundTaskQueue;
        _logger = logger;
    }

    private static AsientoContableDto ToDto(AsientoContable a) => new(
        a.Id, a.Descripcion, a.CuentaDebitoId, a.CuentaCreditoId,
        a.FechaAsiento, a.MontoAsiento, a.Estado, a.OrdenCompraNumero, a.FechaEnvio, a.MensajeError, a.Asiento);

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

        // El WS externo de Render puede tardar hasta 50s en despertar.
        // Esto causa un Error 504 Gateway Timeout en proxies (como AWS / Nginx)
        // cuyo timeout máximo suele ser de 30s.
        // Solución: aceptar la solicitud ya, y encolar el envío para que lo procese
        // QueuedHostedService en segundo plano (con seguimiento del host, no fire-and-forget).

        // Persistimos el estado "enviando" ANTES de encolar el trabajo, para que la
        // única escritura posterior sobre este asiento sea la del propio trabajo en segundo plano.
        asiento.Estado = EstadoAsientoContable.Pendiente;
        asiento.MensajeError = "Enviando en segundo plano (esperando a Render)...";
        await _context.SaveChangesAsync();

        _backgroundTaskQueue.QueueBackgroundWorkItem(async (services, ct) =>
        {
            try
            {
                var scopedContext = services.GetRequiredService<AppDbContext>();
                var scopedClient = services.GetRequiredService<IContabilidadClient>();

                var scopedAsiento = await scopedContext.AsientosContables.FindAsync(new object[] { id }, ct);
                if (scopedAsiento is null) return;

                var (success, error, numeroAsiento) = await scopedClient.EnviarAsientoAsync(scopedAsiento, ct);

                scopedAsiento.Estado = success ? EstadoAsientoContable.Enviado : EstadoAsientoContable.Error;
                scopedAsiento.FechaEnvio = DateTime.UtcNow;
                scopedAsiento.MensajeError = error;
                scopedAsiento.Asiento = numeroAsiento;

                await scopedContext.SaveChangesAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reenviando el asiento {Id} en segundo plano", id);
            }
        });

        return Accepted(ToDto(asiento));
    }
}
