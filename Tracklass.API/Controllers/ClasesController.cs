using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracklass.API;
using Tracklass.API.Models;
using Tracklass.API.Services;

namespace Tracklass.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ClasesController : ControllerBase
    {
        private readonly TracklassDbContext _context;
        private readonly decimal _precioDefault;
        private readonly AuthService _authService;

        public ClasesController(TracklassDbContext context, IConfiguration config, AuthService authService)
        {
            _context = context;
            _precioDefault = config.GetValue<decimal>("ClaseControl:PrecioPorClase", 300);
            _authService = authService;
        }

        private Guid GetUserId() => _authService.GetUserIdFromClaims(User)
            ?? throw new UnauthorizedAccessException();

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var userId = GetUserId();
            var query = _context.Clases
                .Include(c => c.Alumno)
                .Where(c => c.Alumno!.UsuarioId == userId)
                .AsQueryable();

            if (estado != null)
            {
                if (estado == "programadas") query = query.Where(c => c.Estado == EstadoClase.Programada);
                else if (estado == "realizadas") query = query.Where(c => c.Estado == EstadoClase.Realizada);
                else if (estado == "canceladas") query = query.Where(c => c.Estado == EstadoClase.Cancelada);
            }

            if (desde.HasValue)
                query = query.Where(c => c.Fecha.Date >= desde.Value.Date);
            if (hasta.HasValue)
                query = query.Where(c => c.Fecha.Date <= hasta.Value.Date);

            var clases = await query.OrderBy(c => c.Fecha).ThenBy(c => c.HoraInicio).ToListAsync();

            var result = clases.Select(c => new
            {
                id = c.Id,
                alumnoId = c.AlumnoId,
                alumnoNombre = c.Alumno?.Nombre,
                materia = c.Alumno?.Materia,
                fecha = c.Fecha,
                horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                duracionMinutos = c.DuracionMinutos,
                estado = c.Estado.ToString(),
                precio = c.Precio,
                notas = c.Notas,
                pagada = c.Pagada,
                fechaPago = c.FechaPago
            });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = result,
                Message = "Listado de clases"
            });
        }

        [HttpGet("alumno/{alumnoId}")]
        public async Task<IActionResult> GetByAlumno(Guid alumnoId)
        {
            var userId = GetUserId();

            // Verify the alumno belongs to this user
            var alumnoExists = await _context.Alumnos.AnyAsync(a => a.Id == alumnoId && a.UsuarioId == userId);
            if (!alumnoExists)
                return NotFound(new Response<object> { IsSuccess = false, Message = "Alumno no encontrado" });

            var clases = await _context.Clases
                .Where(c => c.AlumnoId == alumnoId)
                .OrderByDescending(c => c.Fecha)
                .ThenBy(c => c.HoraInicio)
                .ToListAsync();

            var result = clases.Select(c => new
            {
                id = c.Id,
                alumnoId = c.AlumnoId,
                fecha = c.Fecha,
                horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                duracionMinutos = c.DuracionMinutos,
                estado = c.Estado.ToString(),
                precio = c.Precio,
                notas = c.Notas,
                pagada = c.Pagada,
                fechaPago = c.FechaPago
            });


            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = result,
                Message = "Clases del alumno"
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var userId = GetUserId();
            var clase = await _context.Clases
                .Include(c => c.Alumno)
                .FirstOrDefaultAsync(c => c.Id == id && c.Alumno!.UsuarioId == userId);

            if (clase == null)
                return NotFound(new Response<Clase> { IsSuccess = false, Message = "Clase no encontrada" });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new
                {
                    id = clase.Id,
                    alumnoId = clase.AlumnoId,
                    alumnoNombre = clase.Alumno?.Nombre,
                    materia = clase.Alumno?.Materia,
                    fecha = clase.Fecha,
                    horaInicio = clase.HoraInicio.ToString(@"hh\:mm"),
                    duracionMinutos = clase.DuracionMinutos,
                    estado = clase.Estado.ToString(),
                    precio = clase.Precio,
                    notas = clase.Notas,
                    pagada = clase.Pagada,
                    fechaPago = clase.FechaPago
                },
                Message = "Clase encontrada"
            });
        }

        [HttpGet("horarios-ocupados")]
        public async Task<IActionResult> GetHorariosOcupados([FromQuery] DateTime fecha)
        {
            var userId = GetUserId();
            var clasesDelDia = await _context.Clases
                .Include(c => c.Alumno)
                .Where(c => c.Fecha.Date == fecha.Date &&
                            c.Estado != EstadoClase.Cancelada &&
                            c.Alumno!.UsuarioId == userId)
                .ToListAsync();

            var horariosOcupados = new List<string>();

            foreach (var c in clasesDelDia)
            {
                var inicio = c.Fecha.Date + c.HoraInicio;
                var fin = inicio.AddMinutes(c.DuracionMinutos);

                while (inicio < fin)
                {
                    horariosOcupados.Add(inicio.ToString("HH:mm"));
                    inicio = inicio.AddMinutes(30);
                }
            }

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = horariosOcupados
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ClaseCrearActualizar model)
        {
            var userId = GetUserId();
            var alumno = await _context.Alumnos.FirstOrDefaultAsync(a => a.Id == model.AlumnoId && a.UsuarioId == userId);

            if (alumno == null)
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Alumno no encontrado"
                });

            // Validar duración mínima y múltiplos de 30
            if (model.DuracionMinutos < 60 || model.DuracionMinutos % 30 != 0)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "La duración mínima es 60 minutos y debe ser múltiplo de 30"
                });
            }


            var nuevaInicio = model.Fecha.Date + model.HoraInicio;
            var nuevaFin = nuevaInicio.AddMinutes(model.DuracionMinutos);

            if (nuevaInicio < DateTime.Now)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No se puede crear una clase en el pasado"
                });
            }

            // Traemos SOLO clases del día del usuario
            var clasesDelDia = await _context.Clases
                .Include(c => c.Alumno)
                .Where(c =>
                    c.Fecha.Date == model.Fecha.Date &&
                    c.Estado != EstadoClase.Cancelada &&
                    c.Alumno!.UsuarioId == userId)
                .ToListAsync();

            // Validamos solapamiento en memoria
            var existeSolapamiento = clasesDelDia.Any(c =>
            {
                var existenteInicio = c.Fecha.Date + c.HoraInicio;
                var existenteFin = existenteInicio.AddMinutes(c.DuracionMinutos);

                return nuevaInicio < existenteFin &&
                       nuevaFin > existenteInicio;
            });

            if (existeSolapamiento)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Ya existe una clase que se solapa con ese horario"
                });
            }

            var precio = model.Precio > 0 ? model.Precio : _precioDefault;

            var clase = new Clase
            {
                AlumnoId = model.AlumnoId,
                Fecha = model.Fecha,
                HoraInicio = model.HoraInicio,
                DuracionMinutos = model.DuracionMinutos,
                Estado = model.Estado,
                Precio = precio,
                Notas = model.Notas,
                Pagada = model.Pagada,
                FechaPago = model.FechaPago
            };

            await _context.Clases.AddAsync(clase);
            await _context.SaveChangesAsync();

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new
                {
                    id = clase.Id,
                    alumnoNombre = alumno.Nombre,
                    fecha = clase.Fecha,
                    horaInicio = clase.HoraInicio.ToString(@"hh\:mm"),
                    estado = clase.Estado.ToString(),
                    pagada = clase.Pagada
                },
                Message = "Clase creada correctamente"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] ClaseCrearActualizar model)
        {
            var userId = GetUserId();
            var clase = await _context.Clases
                .Include(c => c.Alumno)
                .FirstOrDefaultAsync(c => c.Id == id && c.Alumno!.UsuarioId == userId);

            if (clase == null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Clase no encontrada"
                });

            // Si ya está realizada, no permitir cambios de horario
            if (clase.Estado == EstadoClase.Realizada && model.Estado != EstadoClase.Realizada)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No se puede cambiar el estado de una clase ya realizada"
                });
            }

            // CASO ESPECIAL: solo marcar como realizada
            if (model.Estado == EstadoClase.Realizada && clase.Estado != EstadoClase.Realizada)
            {
                clase.Estado = EstadoClase.Realizada;
            }

            // Validar duración mínima y múltiplos de 30
            if (model.DuracionMinutos < 60 || model.DuracionMinutos % 30 != 0)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "La duración mínima es 60 minutos y debe ser múltiplo de 30"
                });
            }

            var nuevaInicio = model.Fecha.Date + model.HoraInicio;
            var nuevaFin = nuevaInicio.AddMinutes(model.DuracionMinutos);

            // No permitir mover al pasado si no es realizada
            if (nuevaInicio < DateTime.Now && model.Estado != EstadoClase.Realizada)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No se puede mover una clase al pasado"
                });
            }

            // Validar solapamiento
            var clasesDelDia = await _context.Clases
                .Include(c => c.Alumno)
                .Where(c =>
                    c.Id != id &&
                    c.Fecha.Date == model.Fecha.Date &&
                    c.Estado != EstadoClase.Cancelada &&
                    c.Alumno!.UsuarioId == userId)
                .ToListAsync();

            var existeSolapamiento = clasesDelDia.Any(c =>
            {
                var existenteInicio = c.Fecha.Date + c.HoraInicio;
                var existenteFin = existenteInicio.AddMinutes(c.DuracionMinutos);

                return nuevaInicio < existenteFin &&
                       nuevaFin > existenteInicio;
            });

            if (existeSolapamiento)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Ya existe otra clase que se solapa con ese horario"
                });
            }

            // 🔹 Actualización normal
            clase.AlumnoId = model.AlumnoId;
            clase.Fecha = model.Fecha;
            clase.HoraInicio = model.HoraInicio;
            clase.DuracionMinutos = model.DuracionMinutos;
            clase.Estado = model.Estado;
            clase.Precio = model.Precio;
            clase.Notas = model.Notas;
            clase.Pagada = model.Pagada;
            clase.FechaPago = model.FechaPago;

            await _context.SaveChangesAsync();

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "Clase actualizada correctamente"
            });
        }


        [HttpPatch("{id}/pago")]
        public async Task<IActionResult> ActualizarPago(Guid id, [FromBody] bool pagada)
        {
            var userId = GetUserId();
            var clase = await _context.Clases
                .Include(c => c.Alumno)
                .FirstOrDefaultAsync(c => c.Id == id && c.Alumno!.UsuarioId == userId);

            if (clase == null)
                return NotFound(new Response<object> { IsSuccess = false, Message = "Clase no encontrada" });

            clase.Pagada = pagada;
            clase.FechaPago = pagada ? DateTime.UtcNow : null;

            await _context.SaveChangesAsync();

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = pagada ? "Clase marcada como pagada" : "Clase marcada como pendiente",
                Result = new { pagada = clase.Pagada, fechaPago = clase.FechaPago }
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = GetUserId();
            var clase = await _context.Clases
                .Include(c => c.Alumno)
                .FirstOrDefaultAsync(c => c.Id == id && c.Alumno!.UsuarioId == userId);

            if (clase == null)
                return NotFound(new Response<Clase> { IsSuccess = false, Message = "Clase no encontrada" });

            _context.Clases.Remove(clase);
            await _context.SaveChangesAsync();

            return Ok(new Response<Clase> { IsSuccess = true, Result = clase, Message = "Clase eliminada" });
        }
    }
}
