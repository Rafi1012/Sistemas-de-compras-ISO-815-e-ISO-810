using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaDeCompras.Data;
using SistemaDeCompras.DTOs;
using SistemaDeCompras.Models.Entities;
using SistemaDeCompras.Models.Enums;
using SistemaDeCompras.Services;

namespace SistemaDeCompras.Controllers;

[ApiController]
[Route("api/auth")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(AppDbContext context, IJwtTokenService jwtTokenService)
    {
        _context = context;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("registro")]
    public async Task<ActionResult<AuthResponseDto>> Registro(RegisterDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.NombreUsuario) || string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
            return BadRequest(new { message = "Usuario, correo y contraseña son requeridos." });

        if (dto.Password.Length < 6)
            return BadRequest(new { message = "La contraseña debe tener al menos 6 caracteres." });

        if (await _context.Usuarios.AnyAsync(u => u.NombreUsuario == dto.NombreUsuario))
            return BadRequest(new { message = $"Ya existe un usuario con el nombre {dto.NombreUsuario}." });

        if (await _context.Usuarios.AnyAsync(u => u.Email == dto.Email))
            return BadRequest(new { message = $"Ya existe un usuario registrado con el correo {dto.Email}." });

        var (hash, salt) = PasswordHasher.Hash(dto.Password);
        var usuario = new Usuario
        {
            NombreUsuario = dto.NombreUsuario,
            Email = dto.Email,
            PasswordHash = hash,
            PasswordSalt = salt
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync();

        var (token, expiraEn) = _jwtTokenService.GenerarToken(usuario);
        return Ok(new AuthResponseDto(token, expiraEn, usuario.Id, usuario.NombreUsuario, usuario.Email));
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto dto)
    {
        var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.NombreUsuario == dto.NombreUsuario);
        if (usuario is null || usuario.Estado != EstadoRegistro.Activo || !PasswordHasher.Verify(dto.Password, usuario.PasswordHash, usuario.PasswordSalt))
            return Unauthorized(new { message = "Usuario o contraseña incorrectos." });

        var (token, expiraEn) = _jwtTokenService.GenerarToken(usuario);
        return Ok(new AuthResponseDto(token, expiraEn, usuario.Id, usuario.NombreUsuario, usuario.Email));
    }
}
