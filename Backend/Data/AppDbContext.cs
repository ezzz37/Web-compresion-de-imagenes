using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> opciones)
            : base(opciones)
        { }

        public DbSet<Imagen> Imagenes { get; set; }
        public DbSet<ImagenProcesada> ImagenesProcesadas { get; set; }
        public DbSet<AlgoritmoCompresion> AlgoritmosCompresion { get; set; }
        public DbSet<Comparacion> Comparaciones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelo)
        {
            // Mapear nombres de tablas
            modelo.Entity<Imagen>().ToTable("Imagenes");
            modelo.Entity<ImagenProcesada>().ToTable("ImagenesProcesadas");
            modelo.Entity<AlgoritmoCompresion>().ToTable("AlgoritmosCompresion");
            modelo.Entity<Comparacion>().ToTable("Comparaciones");

            // Relaciones y claves foraneas
            modelo.Entity<ImagenProcesada>()
                  .HasOne(ip => ip.ImagenOriginal)
                  .WithMany(img => img.ImagenesProcesadas)
                  .HasForeignKey(ip => ip.IdImagenOriginal)
                  .OnDelete(DeleteBehavior.Cascade);

            modelo.Entity<ImagenProcesada>()
                  .HasOne(ip => ip.AlgoritmoCompresion)
                  .WithMany(a => a.ImagenesProcesadas)
                  .HasForeignKey(ip => ip.IdAlgoritmoCompresion);

            modelo.Entity<Comparacion>()
                  .HasOne(c => c.ImagenOriginal)
                  .WithMany(img => img.ComparacionesOriginal)
                  .HasForeignKey(c => c.IdImagenOriginal)
                  .OnDelete(DeleteBehavior.NoAction);

            modelo.Entity<Comparacion>()
                  .HasOne(c => c.ImagenProcesada)
                  .WithMany(ip => ip.Comparaciones)
                  .HasForeignKey(c => c.IdImagenProcesada)
                  .OnDelete(DeleteBehavior.Cascade);

            // Restricción ProfundidadBits
            modelo.Entity<ImagenProcesada>()
                  .HasCheckConstraint("CHK_ProfundidadBits", "[ProfundidadBits] IN (1,8,24)");
        }
    }
}
