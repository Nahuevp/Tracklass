using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracklass.API.Models;
using Tracklass.API.Services;

namespace Tracklass.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly TracklassDbContext _context;
        private readonly AuthService _authService;

        public AuthController(TracklassDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Response<object> { IsSuccess = false, Message = "Datos inválidos" });

            var existingUser = await _context.Usuarios
                .AnyAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (existingUser)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Ya existe un usuario con ese email"
                });
            }

            var usuario = new Usuario
            {
                Email = model.Email.ToLower().Trim(),
                PasswordHash = _authService.HashPassword(model.Password),
                Nombre = model.Nombre.Trim()
            };

            await _context.Usuarios.AddAsync(usuario);
            await _context.SaveChangesAsync();

            var token = _authService.GenerateToken(usuario);

            return Ok(new Response<AuthResponse>
            {
                IsSuccess = true,
                Result = new AuthResponse
                {
                    Token = token,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre
                },
                Message = "Usuario registrado correctamente"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Response<object> { IsSuccess = false, Message = "Datos inválidos" });

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.ToLower());

            if (usuario == null || !_authService.VerifyPassword(model.Password, usuario.PasswordHash))
            {
                return Unauthorized(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Email o contraseña incorrectos"
                });
            }

            var token = _authService.GenerateToken(usuario);

            return Ok(new Response<AuthResponse>
            {
                IsSuccess = true,
                Result = new AuthResponse
                {
                    Token = token,
                    Email = usuario.Email,
                    Nombre = usuario.Nombre
                },
                Message = "Login exitoso"
            });
        }

        [HttpGet("me")]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> Me()
        {
            var userId = _authService.GetUserIdFromClaims(User);
            if (userId == null)
                return Unauthorized(new Response<object> { IsSuccess = false, Message = "Token inválido" });

            var usuario = await _context.Usuarios.FindAsync(userId.Value);
            if (usuario == null)
                return NotFound(new Response<object> { IsSuccess = false, Message = "Usuario no encontrado" });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new
                {
                    id = usuario.Id,
                    email = usuario.Email,
                    nombre = usuario.Nombre
                },
                Message = "Usuario autenticado"
            });
        }
    }
}
