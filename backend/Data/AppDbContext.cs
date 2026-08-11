using Microsoft.EntityFrameworkCore;
using SistemaDeCompras.Models.Entities;
using SistemaDeCompras.Models.Enums;

namespace SistemaDeCompras.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Departamento> Departamentos => Set<Departamento>();
    public DbSet<UnidadMedida> UnidadesMedida => Set<UnidadMedida>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Articulo> Articulos => Set<Articulo>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraDetalle> DetallesOrdenCompra => Set<OrdenCompraDetalle>();
    public DbSet<AsientoContable> AsientosContables => Set<AsientoContable>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departamento>(e =>
        {
            e.HasIndex(d => d.Nombre).IsUnique();
        });

        modelBuilder.Entity<UnidadMedida>(e =>
        {
            e.HasIndex(u => u.Descripcion).IsUnique();
        });

        modelBuilder.Entity<Proveedor>(e =>
        {
            e.HasIndex(p => p.CedulaRnc).IsUnique();
            e.Property(p => p.CedulaRnc).HasMaxLength(20);
        });

        modelBuilder.Entity<Articulo>(e =>
        {
            e.Property(a => a.Existencia).HasColumnType("decimal(18,2)");
            e.HasOne(a => a.UnidadMedida)
                .WithMany(u => u.Articulos)
                .HasForeignKey(a => a.UnidadMedidaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Empleado>(e =>
        {
            e.HasOne(emp => emp.Departamento)
                .WithMany(d => d.Empleados)
                .HasForeignKey(emp => emp.DepartamentoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrdenCompra>(e =>
        {
            e.HasKey(o => o.Numero);
            e.Property(o => o.Numero).ValueGeneratedOnAdd();

            e.HasOne(o => o.Proveedor)
                .WithMany(p => p.OrdenesCompra)
                .HasForeignKey(o => o.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(o => o.Departamento)
                .WithMany(d => d.OrdenesCompra)
                .HasForeignKey(o => o.DepartamentoId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(o => o.Empleado)
                .WithMany(emp => emp.OrdenesCompra)
                .HasForeignKey(o => o.EmpleadoId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<OrdenCompraDetalle>(e =>
        {
            e.Property(d => d.Cantidad).HasColumnType("decimal(18,2)");
            e.Property(d => d.CostoUnitario).HasColumnType("decimal(18,2)");

            e.HasOne(d => d.OrdenCompra)
                .WithMany(o => o.Detalles)
                .HasForeignKey(d => d.OrdenCompraNumero)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(d => d.Articulo)
                .WithMany(a => a.DetallesOrdenCompra)
                .HasForeignKey(d => d.ArticuloId)
                .OnDelete(DeleteBehavior.SetNull);

            e.HasOne(d => d.UnidadMedida)
                .WithMany()
                .HasForeignKey(d => d.UnidadMedidaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AsientoContable>(e =>
        {
            e.Property(a => a.MontoAsiento).HasColumnType("decimal(18,2)");

            e.HasOne(a => a.OrdenCompra)
                .WithMany(o => o.AsientosContables)
                .HasForeignKey(a => a.OrdenCompraNumero)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.NombreUsuario).IsUnique();
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.NombreUsuario).HasMaxLength(50);
            e.Property(u => u.Email).HasMaxLength(150);
        });

        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Departamento>().HasData(
            new Departamento { Id = 1, Nombre = "Compras", Estado = EstadoRegistro.Activo },
            new Departamento { Id = 2, Nombre = "Contabilidad", Estado = EstadoRegistro.Activo },
            new Departamento { Id = 3, Nombre = "Almacen", Estado = EstadoRegistro.Activo }
        );

        modelBuilder.Entity<UnidadMedida>().HasData(
            new UnidadMedida { Id = 1, Descripcion = "Unidad", Estado = EstadoRegistro.Activo },
            new UnidadMedida { Id = 2, Descripcion = "Caja", Estado = EstadoRegistro.Activo },
            new UnidadMedida { Id = 3, Descripcion = "Libra", Estado = EstadoRegistro.Activo }
        );

        modelBuilder.Entity<Proveedor>().HasData(
            new Proveedor { Id = 1, CedulaRnc = "101000001", NombreComercial = "Suplidora Nacional SRL", Estado = EstadoRegistro.Activo },
            new Proveedor { Id = 2, CedulaRnc = "101000002", NombreComercial = "Distribuidora Caribe SRL", Estado = EstadoRegistro.Activo }
        );

        modelBuilder.Entity<Empleado>().HasData(
            new Empleado { Id = 1, Nombre = "Juan Perez", DepartamentoId = 1, Estado = EstadoRegistro.Activo },
            new Empleado { Id = 2, Nombre = "Maria Gomez", DepartamentoId = 3, Estado = EstadoRegistro.Activo }
        );

        modelBuilder.Entity<Articulo>().HasData(
            new Articulo { Id = 1, Descripcion = "Papel Bond 8.5x11", Marca = "Report", UnidadMedidaId = 2, Existencia = 50, Estado = EstadoRegistro.Activo },
            new Articulo { Id = 2, Descripcion = "Tinta para impresora", Marca = "HP", UnidadMedidaId = 1, Existencia = 20, Estado = EstadoRegistro.Activo }
        );
    }
}
