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
    public class DashboardController : ControllerBase
    {
        private readonly TracklassDbContext _context;
        private readonly AuthService _authService;

        public DashboardController(TracklassDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;
        }

        private Guid GetUserId() => _authService.GetUserIdFromClaims(User)
            ?? throw new UnauthorizedAccessException();

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] DateTime? fecha)
        {
            try
            {
                var userId = GetUserId();
                var hoy = fecha?.Date ?? DateTime.Today;
                var inicioMes = new DateTime(hoy.Year, hoy.Month, 1);
                var finMes = inicioMes.AddMonths(1).AddDays(-1);

                // Base query scoped to user
                var clasesQuery = _context.Clases
                    .Include(c => c.Alumno)
                    .Where(c => c.Alumno!.UsuarioId == userId);

                var clasesHoy = await clasesQuery
                    .Where(c => c.Fecha.Date == hoy && c.Estado != EstadoClase.Cancelada)
                    .OrderBy(c => c.HoraInicio)
                    .ToListAsync();

                var proximaClase = clasesHoy.FirstOrDefault(c => c.Estado == EstadoClase.Programada)
                    ?? await clasesQuery
                        .Where(c => c.Fecha.Date >= hoy && c.Estado == EstadoClase.Programada)
                        .OrderBy(c => c.Fecha)
                        .ThenBy(c => c.HoraInicio)
                        .FirstOrDefaultAsync();

                var clasesRealizadasMes = await clasesQuery.CountAsync(c =>
                    c.Fecha.Date >= inicioMes &&
                    c.Fecha.Date <= finMes &&
                    c.Estado == EstadoClase.Realizada);

                // INGRESOS REALES (SOLO REALIZADAS Y PAGADAS)
                var ingresosMes = await clasesQuery
                    .Where(c => c.Fecha.Date >= inicioMes
                             && c.Fecha.Date <= finMes
                             && c.Estado == EstadoClase.Realizada
                             && c.Pagada)
                    .SumAsync(c => c.Precio);
                    
                // DEUDA TOTAL (REALIZADAS PERO NO PAGADAS HISTORICAMENTE)
                var deudaTotal = await clasesQuery
                    .Where(c => c.Estado == EstadoClase.Realizada && !c.Pagada)
                    .SumAsync(c => c.Precio);

                // INGRESOS ESTIMADOS (SOLO PROGRAMADAS)
                var ingresosEstimadosMes = await clasesQuery
                    .Where(c => c.Fecha.Date >= inicioMes
                             && c.Fecha.Date <= finMes
                             && c.Estado == EstadoClase.Programada)
                    .SumAsync(c => c.Precio);

                var alumnosActivos = await _context.Alumnos.CountAsync(a => a.Activo && a.UsuarioId == userId);
                var alumnosTotales = await _context.Alumnos.CountAsync(a => a.UsuarioId == userId);

                var proximasClases = await clasesQuery
                    .Where(c => c.Fecha.Date > hoy && c.Estado == EstadoClase.Programada)
                    .OrderBy(c => c.Fecha)
                    .ThenBy(c => c.HoraInicio)
                    .Take(10)
                    .ToListAsync();

                var inicioSemana = hoy;
                while (inicioSemana.DayOfWeek != DayOfWeek.Monday)
                    inicioSemana = inicioSemana.AddDays(-1);

                var finSemana = inicioSemana.AddDays(6);

                var clasesEstaSemana = await clasesQuery.CountAsync(c =>
                    c.Fecha.Date >= inicioSemana &&
                    c.Fecha.Date <= finSemana &&
                    c.Estado != EstadoClase.Cancelada);

                var clasesEsteMes = await clasesQuery.CountAsync(c =>
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
                            estado = c.Estado.ToString(),
                            pagada = c.Pagada
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
                        deudaTotal,
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
            catch (Exception ex)
            {
                return StatusCode(500, new Response<object>
                {
                    IsSuccess = false,
                    Message = ex.ToString()
                });
            }
        }

    }
}
