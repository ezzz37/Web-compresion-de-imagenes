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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // ---------------------------------------------------
            // 1. Mapear cada entidad a su tabla correspondiente
            // ---------------------------------------------------
            modelBuilder.Entity<Imagen>().ToTable("Imagenes");
            modelBuilder.Entity<ImagenProcesada>().ToTable("ImagenesProcesadas");
            modelBuilder.Entity<AlgoritmoCompresion>().ToTable("AlgoritmosCompresion");
            modelBuilder.Entity<Comparacion>().ToTable("Comparaciones");

            // ---------------------------------------------------
            // 2. Configurar claves primarias explícitas (opcional,
            //    EF las detecta automáticamente por convención si la
            //    propiedad se llama IdEntidad o EntidadId)
            // ---------------------------------------------------
            modelBuilder.Entity<Imagen>()
                        .HasKey(i => i.IdImagen);

            modelBuilder.Entity<ImagenProcesada>()
                        .HasKey(ip => ip.IdImagenProcesada);

            modelBuilder.Entity<AlgoritmoCompresion>()
                        .HasKey(a => a.IdAlgoritmoCompresion);

            modelBuilder.Entity<Comparacion>()
                        .HasKey(c => c.IdComparacion);

            // ---------------------------------------------------
            // 3. Relaciones y claves foráneas
            // ---------------------------------------------------

            // 3.1. ImagenProcesada → Imagen (ImagenOriginal)
            modelBuilder.Entity<ImagenProcesada>()
                .HasOne(ip => ip.ImagenOriginal)
                .WithMany(img => img.ImagenesProcesadas)
                .HasForeignKey(ip => ip.IdImagenOriginal)
                .OnDelete(DeleteBehavior.Cascade);

            // 3.2. ImagenProcesada → AlgoritmoCompresion
            modelBuilder.Entity<ImagenProcesada>()
                .HasOne(ip => ip.AlgoritmoCompresion)
                .WithMany(a => a.ImagenesProcesadas)
                .HasForeignKey(ip => ip.IdAlgoritmoCompresion)
                // Si borras un algoritmo, no quieres que borre en cascada las imágenes procesadas:
                .OnDelete(DeleteBehavior.Restrict);

            // 3.3. Comparacion → Imagen (ImagenOriginal)
            modelBuilder.Entity<Comparacion>()
                .HasOne(c => c.ImagenOriginal)
                .WithMany(img => img.ComparacionesOriginal)
                .HasForeignKey(c => c.IdImagenOriginal)
                // Al eliminar una Imagen original, NO eliminar todas sus comparaciones automáticamente:
                .OnDelete(DeleteBehavior.Restrict);

            // 3.4. Comparacion → ImagenProcesada
            modelBuilder.Entity<Comparacion>()
                .HasOne(c => c.ImagenProcesada)
                .WithMany(ip => ip.Comparaciones)
                .HasForeignKey(c => c.IdImagenProcesada)
                // Si eliminas la ImagenProcesada, eliminar en cascada las comparaciones asociadas:
                .OnDelete(DeleteBehavior.Cascade);

            // ---------------------------------------------------
            // 4. Configuración de tipos y restricciones adicionales
            // ---------------------------------------------------

            // 4.1. Asegurar que ImagenDiferencias sea VARBINARY(MAX)
            modelBuilder.Entity<Comparacion>()
                .Property(c => c.ImagenDiferencias)
                .HasColumnType("varbinary(max)");

            // 4.2. Check constraint para ProfundidadBits en ImagenProcesada
            modelBuilder.Entity<ImagenProcesada>()
                .HasCheckConstraint(
                    name: "CHK_ProfundidadBits",
                    sql: "[ProfundidadBits] IN (1, 8, 24)");

            // Finalmente, invocar el base para que EF complete la configuración interna
            base.OnModelCreating(modelBuilder);
        }
    }
}
