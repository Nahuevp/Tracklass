using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracklass.API;
using Tracklass.API.Models;

namespace Tracklass.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AlumnosController(TracklassDbContext context) : ControllerBase
    {
        private readonly TracklassDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] string? search, [FromQuery] string? estado)
        {
            var query = _context.Alumnos.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var s = search.ToLower();
                query = query.Where(a => a.Nombre.ToLower().Contains(s) || (a.Email != null && a.Email.ToLower().Contains(s)) || a.Materia.ToLower().Contains(s));
            }

            if (estado == "activos")
                query = query.Where(a => a.Activo);
            else if (estado == "inactivos")
                query = query.Where(a => !a.Activo);

            var alumnos = await query.OrderBy(a => a.Nombre).ToListAsync();

            var result = alumnos.Select(a => new
            {
                id = a.Id,
                nombre = a.Nombre,
                email = a.Email,
                materia = a.Materia,
                activo = a.Activo,
                telefono = a.Telefono,
                ultimaClase = _context.Clases
                    .Where(c => c.AlumnoId == a.Id && c.Estado == EstadoClase.Realizada)
                    .OrderByDescending(c => c.Fecha)
                    .Select(c => c.Fecha)
                    .FirstOrDefault(),
                clasesTotales = _context.Clases.Count(c => c.AlumnoId == a.Id && c.Estado == EstadoClase.Realizada)
            });

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = result,
                Message = "Listado de alumnos"
            });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
                return NotFound(new Response<Alumno> { IsSuccess = false, Message = "Alumno no encontrado" });

            var ultimaClase = await _context.Clases
                .Where(c => c.AlumnoId == id && c.Estado == EstadoClase.Realizada)
                .OrderByDescending(c => c.Fecha)
                .Select(c => c.Fecha)
                .FirstOrDefaultAsync();

            var clasesTotales = await _context.Clases.CountAsync(c => c.AlumnoId == id && c.Estado == EstadoClase.Realizada);

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new
                {
                    id = alumno.Id,
                    nombre = alumno.Nombre,
                    email = alumno.Email,
                    materia = alumno.Materia,
                    activo = alumno.Activo,
                    telefono = alumno.Telefono,
                    notas = alumno.Notas,
                    ultimaClase,
                    clasesTotales
                },
                Message = "Alumno encontrado"
            });
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] AlumnoCrearActualizar model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new Response<AlumnoCrearActualizar> { IsSuccess = false, Result = model, Message = "Campos inválidos" });

            var materia = model.Materia?.Trim().ToLower();

            if (materia != "ingles" && materia != "informatica")
            {
                return BadRequest(new Response<AlumnoCrearActualizar>
                {
                    IsSuccess = false,
                    Result = model,
                    Message = "La materia debe ser Ingles o Informatica"
                });
            }

            var alumno = new Alumno
            {
                Nombre = model.Nombre,
                Email = model.Email,
                Materia = model.Materia.Trim(),
                Activo = model.Activo,
                Telefono = model.Telefono,
                Notas = model.Notas
            };
            await _context.Alumnos.AddAsync(alumno);
            await _context.SaveChangesAsync();

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new { id = alumno.Id, nombre = alumno.Nombre, email = alumno.Email, materia = alumno.Materia, activo = alumno.Activo, telefono = alumno.Telefono },
                Message = "Alumno creado correctamente"
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] AlumnoCrearActualizar model)
        {
            if (id == Guid.Empty)
                return BadRequest(new Response<AlumnoCrearActualizar> { IsSuccess = false, Result = model, Message = "ID inválido" });

            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
                return NotFound(new Response<AlumnoCrearActualizar> { IsSuccess = false, Result = model, Message = "Alumno no encontrado" });

            if (model.Materia != "Ingles" && model.Materia != "Informatica")
                return BadRequest(new Response<AlumnoCrearActualizar> { IsSuccess = false, Result = model, Message = "La materia debe ser Ingles o Informatica" });

            alumno.Nombre = model.Nombre;
            alumno.Email = model.Email;
            alumno.Materia = model.Materia;
            alumno.Activo = model.Activo;
            alumno.Telefono = model.Telefono;
            alumno.Notas = model.Notas;

            _context.Alumnos.Update(alumno);
            await _context.SaveChangesAsync();

            return Ok(new Response<AlumnoCrearActualizar> { IsSuccess = true, Result = model, Message = "Alumno actualizado" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var alumno = await _context.Alumnos.FindAsync(id);
            if (alumno == null)
                return NotFound(new Response<Alumno> { IsSuccess = false, Message = "Alumno no encontrado" });

            var clases = await _context.Clases.Where(c => c.AlumnoId == id).ToListAsync();
            _context.Clases.RemoveRange(clases);
            _context.Alumnos.Remove(alumno);
            await _context.SaveChangesAsync();

            return Ok(new Response<Alumno> { IsSuccess = true, Result = alumno, Message = $"Alumno {alumno.Nombre} eliminado" });
        }
    }
}
