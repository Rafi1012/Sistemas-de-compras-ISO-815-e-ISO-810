namespace SistemaDeCompras.DTOs;

public record RegisterDto(string NombreUsuario, string Email, string Password);

public record LoginDto(string NombreUsuario, string Password);

public record AuthResponseDto(string Token, DateTime ExpiraEn, int UsuarioId, string NombreUsuario, string Email);
