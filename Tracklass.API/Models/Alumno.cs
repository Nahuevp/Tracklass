using System.ComponentModel.DataAnnotations;

namespace Tracklass.API.Models
{
    public class Alumno
    {
        public Guid Id { get; set; }
        
        [Required]
        [MinLength(2)]
        public string Nombre { get; set; } = string.Empty;
        
        [EmailAddress]
        public string? Email { get; set; } = null;
        
        [Required]
        public string Materia { get; set; } = string.Empty; // "Ingles" | "Informatica"
        
        public bool Activo { get; set; } = true;
        
        public string? Telefono { get; set; }
        
        public string? Notas { get; set; }
        
        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    }

    public class AlumnoCrearActualizar
    {
        [Required(ErrorMessage = "El nombre es requerido")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string Nombre { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email inválido")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "La materia es requerida")]
        public string Materia { get; set; } = string.Empty; // "Ingles" | "Informatica"

        public bool Activo { get; set; } = true;

        public string? Telefono { get; set; }

        public string? Notas { get; set; }
    }
}
