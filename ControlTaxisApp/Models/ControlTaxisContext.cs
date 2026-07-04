using Microsoft.EntityFrameworkCore;
using System.Collections;

namespace ControlTaxisApp.Models
{
    public class ControlTaxisContext : DbContext
    {
        public ControlTaxisContext(DbContextOptions<ControlTaxisContext> options) : base(options) { }

        public DbSet<Mantenimiento> Mantenimientos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Vehiculo> Vehiculos { get; set; }
        public DbSet<Conductor> Conductores { get; set; }
        public DbSet<LiquidacionDiaria> LiquidacionesDiarias { get; set; }

        public DbSet<TipoMantenimiento> TiposMantenimiento { get; set; }

        // En tu archivo ControlTaxisContext.cs
        public DbSet<Festivo> Festivos { get; set; }
        // public IEnumerable TiposMantenimiento { get; internal set; }

        // === SÚPER IMPORTANTE: AGREGA ESTE BLOQUE AQUÍ ===
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Le decimos explícitamente a Entity Framework que remueva los plurales automáticos 
            // y busque los nombres exactos de las tablas tal y como se las pedimos
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");
            modelBuilder.Entity<Vehiculo>().ToTable("Vehiculos");
            modelBuilder.Entity<Conductor>().ToTable("Conductores");
            modelBuilder.Entity<LiquidacionDiaria>().ToTable("LiquidacionesDiarias");


            modelBuilder.Entity<TipoMantenimiento>().HasData(
        new TipoMantenimiento { Id = 1, Nombre = "Motor" },
        new TipoMantenimiento { Id = 2, Nombre = "Caja de Cambios" },
        new TipoMantenimiento { Id = 3, Nombre = "Frenos" },
        new TipoMantenimiento { Id = 4, Nombre = "Suspensión" },
        new TipoMantenimiento { Id = 5, Nombre = "Sistema Eléctrico" },
        new TipoMantenimiento { Id = 6, Nombre = "Llantas" },
        new TipoMantenimiento { Id = 7, Nombre = "Cambio de Aceite" },
        new TipoMantenimiento { Id = 8, Nombre = "Aire Acondicionado" },
        new TipoMantenimiento { Id = 9, Nombre = "Otro" }
    );

        }
    }
}