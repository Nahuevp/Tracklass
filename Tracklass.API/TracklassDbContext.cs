using Microsoft.EntityFrameworkCore;
using Tracklass.API.Models;

namespace Tracklass.API
{
    public class  TracklassDbContext: DbContext
    {
        public TracklassDbContext(DbContextOptions<TracklassDbContext> op): base(op)
        {
            
        }
        public DbSet<Alumno> Alumnos { get; set; }
        public DbSet<Clase> Clases { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Clase>()
                .HasOne(c => c.Alumno)
                .WithMany()
                .HasForeignKey(c => c.AlumnoId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
