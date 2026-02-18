using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tracklass.API;
using Tracklass.API.Models;

namespace Tracklass.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController(TracklassDbContext context) : ControllerBase
    {
        private readonly TracklassDbContext _context = context;

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? fecha)
        {
            var hoy = fecha?.Date ?? DateTime.Today;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
            var finMes = inicioMes.AddMonths(1).AddDays(-1);

            var clasesHoy = await _context.Clases
                .Include(c => c.Alumno)
                .Where(c => c.Fecha.Date == hoy && c.Estado != EstadoClase.Cancelada)
                .OrderBy(c => c.HoraInicio)
                .ToListAsync();

            var proximaClase = clasesHoy.FirstOrDefault(c => c.Estado == EstadoClase.Programada)
                ?? await _context.Clases
                    .Include(c => c.Alumno)
                    .Where(c => c.Fecha.Date >= hoy && c.Estado == EstadoClase.Programada)
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.HoraInicio)
                    .FirstOrDefaultAsync();

            var clasesRealizadasMes = await _context.Clases.CountAsync(c =>
                c.Fecha.Date >= inicioMes &&
                c.Fecha.Date <= finMes &&
                c.Estado == EstadoClase.Realizada);

            // INGRESOS REALES (SOLO REALIZADAS)
            var ingresosMes = await _context.Clases
                .Where(c => c.Fecha.Date >= inicioMes
                         && c.Fecha.Date <= finMes
                         && c.Estado == EstadoClase.Realizada)
                .SumAsync(c => c.Precio);

            // INGRESOS ESTIMADOS (SOLO PROGRAMADAS)
            var ingresosEstimadosMes = await _context.Clases
                .Where(c => c.Fecha.Date >= inicioMes
                         && c.Fecha.Date <= finMes
                         && c.Estado == EstadoClase.Programada)
                .SumAsync(c => c.Precio);

            var alumnosActivos = await _context.Alumnos.CountAsync(a => a.Activo);
            var alumnosTotales = await _context.Alumnos.CountAsync();

            var proximasClases = await _context.Clases
                .Include(c => c.Alumno)
                .Where(c => c.Fecha.Date > hoy && c.Estado == EstadoClase.Programada)
                .OrderBy(c => c.Fecha)
                .ThenBy(c => c.HoraInicio)
                .Take(10)
                .ToListAsync();

            var inicioSemana = hoy;
            while (inicioSemana.DayOfWeek != DayOfWeek.Monday)
                inicioSemana = inicioSemana.AddDays(-1);

            var finSemana = inicioSemana.AddDays(6);

            var clasesEstaSemana = await _context.Clases.CountAsync(c =>
                c.Fecha.Date >= inicioSemana &&
                c.Fecha.Date <= finSemana &&
                c.Estado != EstadoClase.Cancelada);

            var clasesEsteMes = await _context.Clases.CountAsync(c =>
                c.Fecha.Date >= inicioMes &&
                c.Fecha.Date <= finMes &&
                c.Estado != EstadoClase.Cancelada);

            return Ok(new Response<object>
            {
                IsSuccess = true,
                Result = new
                {
                    clasesHoy = clasesHoy.Select(c => new
                    {
                        id = c.Id,
                        alumnoId = c.AlumnoId,
                        alumnoNombre = c.Alumno?.Nombre,
                        materia = c.Alumno?.Materia,
                        fecha = c.Fecha,
                        horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                        duracionMinutos = c.DuracionMinutos,
                        estado = c.Estado.ToString()
                    }),

                    proximaClase = proximaClase != null ? new
                    {
                        alumnoNombre = proximaClase.Alumno?.Nombre,
                        materia = proximaClase.Alumno?.Materia,
                        fecha = proximaClase.Fecha,
                        horaInicio = proximaClase.HoraInicio.ToString(@"hh\:mm")
                    } : null,

                    clasesRealizadasMes,
                    ingresosMes,
                    ingresosEstimadosMes,
                    alumnosActivos,
                    alumnosTotales,

                    proximasClases = proximasClases.Select(c => new
                    {
                        id = c.Id,
                        alumnoId = c.AlumnoId,
                        alumnoNombre = c.Alumno?.Nombre,
                        materia = c.Alumno?.Materia,
                        fecha = c.Fecha,
                        horaInicio = c.HoraInicio.ToString(@"hh\:mm"),
                        duracionMinutos = c.DuracionMinutos
                    }),

                    totalClasesHoy = clasesHoy.Count,

                    resumenAgenda = new
                    {
                        hoy = clasesHoy.Count,
                        estaSemana = clasesEstaSemana,
                        esteMes = clasesEsteMes,
                        ingresoEstimadoMes = ingresosEstimadosMes
                    }
                },
                Message = "Datos del dashboard"
            });
        }

    }
}
