using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracklass.API;
using Tracklass.API.Models;

namespace Tracklass.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClasesController(TracklassDbContext context, IConfiguration config) : ControllerBase
    {
        private readonly TracklassDbContext _context = context;
        private readonly decimal _precioDefault = config.GetValue<decimal>("ClaseControl:PrecioPorClase", 300);

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? estado, [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
        {
            var query = _context.Clases.Include(c => c.Alumno).AsQueryable();

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
                notas = c.Notas
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
                notas = c.Notas
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
            var clase = await _context.Clases.Include(c => c.Alumno).FirstOrDefaultAsync(c => c.Id == id);
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
                    notas = clase.Notas
                },
                Message = "Clase encontrada"
            });
        }

        [HttpGet("horarios-ocupados")]
        public async Task<IActionResult> GetHorariosOcupados([FromQuery] DateTime fecha)
        {
            var clasesDelDia = await _context.Clases
                .Where(c => c.Fecha.Date == fecha.Date &&
                            c.Estado != EstadoClase.Cancelada)
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
            var alumno = await _context.Alumnos.FindAsync(model.AlumnoId);
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

            // Traemos SOLO clases del día
            var clasesDelDia = await _context.Clases
                .Where(c =>
                    c.Fecha.Date == model.Fecha.Date &&
                    c.Estado != EstadoClase.Cancelada)
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
                Notas = model.Notas
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
                    estado = clase.Estado.ToString()
                },
                Message = "Clase creada correctamente"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] ClaseCrearActualizar model)
        {
            var clase = await _context.Clases.FindAsync(id);
            if (clase == null)
                return NotFound(new Response<object>
                {
                    IsSuccess = false,
                    Message = "Clase no encontrada"
                });

            // Si ya está realizada, no permitir cambios
            if (clase.Estado == EstadoClase.Realizada)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No se puede modificar una clase ya realizada"
                });
            }

            // CASO ESPECIAL: solo marcar como realizada
            if (model.Estado == EstadoClase.Realizada)
            {
                clase.Estado = EstadoClase.Realizada;
                await _context.SaveChangesAsync();

                return Ok(new Response<object>
                {
                    IsSuccess = true,
                    Message = "Clase marcada como realizada"
                });
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
            if (nuevaInicio < DateTime.Now)
            {
                return BadRequest(new Response<object>
                {
                    IsSuccess = false,
                    Message = "No se puede mover una clase al pasado"
                });
            }

            // Validar solapamiento
            var clasesDelDia = await _context.Clases
                .Where(c =>
                    c.Id != id &&
                    c.Fecha.Date == model.Fecha.Date &&
                    c.Estado != EstadoClase.Cancelada)
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

            await _context.SaveChangesAsync();

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Message = "Clase actualizada correctamente"
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var clase = await _context.Clases.FindAsync(id);
            if (clase == null)
                return NotFound(new Response<Clase> { IsSuccess = false, Message = "Clase no encontrada" });

            _context.Clases.Remove(clase);
            await _context.SaveChangesAsync();

            return Ok(new Response<Clase> { IsSuccess = true, Result = clase, Message = "Clase eliminada" });
        }
    }
}
