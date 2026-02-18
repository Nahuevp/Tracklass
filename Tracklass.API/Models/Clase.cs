using System.ComponentModel.DataAnnotations;

namespace Tracklass.API.Models
{
    public enum EstadoClase
    {
        Programada = 0,
        Realizada = 1,
        Cancelada = 2
    }

    public class Clase
    {
        public Guid Id { get; set; }
        
        public Guid AlumnoId { get; set; }
        public Alumno? Alumno { get; set; }
        
        [Required]
        public DateTime Fecha { get; set; }
        
        [Required]
        public TimeSpan HoraInicio { get; set; } 
        
        public int DuracionMinutos { get; set; } = 60;
        
        public EstadoClase Estado { get; set; } = EstadoClase.Programada;
        
        public decimal Precio { get; set; } = 300; // Precio por clase en $ Uruguay
        
        public string? Notas { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class ClaseCrearActualizar
    {
        [Required]
        public Guid AlumnoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public TimeSpan HoraInicio { get; set; }

        public int DuracionMinutos { get; set; } = 60;

        public EstadoClase Estado { get; set; } = EstadoClase.Programada;

        public decimal Precio { get; set; } = 300;

        public string? Notas { get; set; }
    }
}
