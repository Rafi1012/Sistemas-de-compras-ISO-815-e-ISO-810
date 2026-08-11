using SistemaDeCompras.Models.Entities;

namespace SistemaDeCompras.Services;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiraEn) GenerarToken(Usuario usuario);
}
