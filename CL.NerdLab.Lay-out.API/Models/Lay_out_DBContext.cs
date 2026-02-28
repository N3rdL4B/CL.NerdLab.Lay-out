using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace CL.NerdLab.Lay_out.API.Models;

public partial class Lay_out_DBContext : DbContext
{
    public Lay_out_DBContext()
    {
    }

    public Lay_out_DBContext(DbContextOptions<Lay_out_DBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Flota> Flota { get; set; }

    public virtual DbSet<HistorialRegistroCarga> HistorialRegistroCarga { get; set; }

    public virtual DbSet<HistorialRegistroCarga_Ledger> HistorialRegistroCarga_Ledger { get; set; }

    public virtual DbSet<LogsActividadUsuarios> LogsActividadUsuarios { get; set; }

    public virtual DbSet<LogsActividadUsuarios_Ledger> LogsActividadUsuarios_Ledger { get; set; }

    public virtual DbSet<PatioSlots> PatioSlots { get; set; }

    public virtual DbSet<PatioZonas> PatioZonas { get; set; }

    public virtual DbSet<Patios> Patios { get; set; }

    public virtual DbSet<RegistroActividadBuses> RegistroActividadBuses { get; set; }

    public virtual DbSet<Roles> Roles { get; set; }

    public virtual DbSet<TipoVehiculo> TipoVehiculo { get; set; }

    public virtual DbSet<Usuarios> Usuarios { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Data Source=(local);Initial Catalog=BD_Lay_out;Integrated Security=False;TrustServerCertificate=True;User ID=sa;Password=N3rdL@B.CL#2025;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flota>(entity =>
        {
            entity.HasKey(e => e.IdPatente).HasName("PK__Flota__9F4EF95C2BBFF81A");

            entity.HasIndex(e => e.Patente, "UQ__Flota__CA655166CCF653FC").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Patente).HasMaxLength(6);

            entity.HasOne(d => d.IdPatioNavigation).WithMany(p => p.Flota)
                .HasForeignKey(d => d.IdPatio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Flota_Patios");

            entity.HasOne(d => d.IdTipoVehiculoNavigation).WithMany(p => p.Flota)
                .HasForeignKey(d => d.IdTipoVehiculo)
                .HasConstraintName("FK_Flota_TipoVehiculo");
        });

        modelBuilder.Entity<HistorialRegistroCarga>(entity =>
        {
            entity.HasKey(e => e.IdRegistroCarga).HasName("PK__Historia__7759F380475D8171");

            entity.Property(e => e.FechaModifRegistro).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdRegistroActividadNavigation).WithMany(p => p.HistorialRegistroCarga)
                .HasForeignKey(d => d.IdRegistroActividad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Historial_Registro");

            entity.HasOne(d => d.IdUsuarioModifNavigation).WithMany(p => p.HistorialRegistroCarga)
                .HasForeignKey(d => d.IdUsuarioModif)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Historial_Usuarios");
        });

        modelBuilder.Entity<HistorialRegistroCarga_Ledger>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("HistorialRegistroCarga_Ledger");

            entity.Property(e => e.IdRegistroCarga).ValueGeneratedOnAdd();
            entity.Property(e => e.ledger_operation_type_desc).HasMaxLength(6);
        });

        modelBuilder.Entity<LogsActividadUsuarios>(entity =>
        {
            entity.HasKey(e => e.IdLog).HasName("PK__LogsActi__0C54DBC670D85210");

            entity.Property(e => e.Accion).HasMaxLength(255);
            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.LogsActividadUsuarios)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Logs_Usuarios");
        });

        modelBuilder.Entity<LogsActividadUsuarios_Ledger>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("LogsActividadUsuarios_Ledger");

            entity.Property(e => e.Accion).HasMaxLength(255);
            entity.Property(e => e.IdLog).ValueGeneratedOnAdd();
            entity.Property(e => e.ledger_operation_type_desc).HasMaxLength(6);
        });

        modelBuilder.Entity<PatioSlots>(entity =>
        {
            entity.HasKey(e => e.IdSlot).HasName("PK__PatioSlo__AC137DE5120EA22E");

            entity.HasOne(d => d.IdPatenteNavigation).WithMany(p => p.PatioSlots)
                .HasForeignKey(d => d.IdPatente)
                .HasConstraintName("FK_Slots_Flota");

            entity.HasOne(d => d.IdZonaNavigation).WithMany(p => p.PatioSlots)
                .HasForeignKey(d => d.IdZona)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Slots_Zonas");
        });

        modelBuilder.Entity<PatioZonas>(entity =>
        {
            entity.HasKey(e => e.IdZona).HasName("PK__PatioZon__F631C12D7BC34222");

            entity.Property(e => e.ColorHex)
                .HasMaxLength(20)
                .HasDefaultValue("rgba(0,0,0,0.05)");
            entity.Property(e => e.Columnas).HasDefaultValue(2);
            entity.Property(e => e.Filas).HasDefaultValue(5);
            entity.Property(e => e.NombreZona).HasMaxLength(100);
            entity.Property(e => e.Orientacion)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasDefaultValue("V")
                .IsFixedLength();

            entity.HasOne(d => d.IdPatioNavigation).WithMany(p => p.PatioZonas)
                .HasForeignKey(d => d.IdPatio)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Zonas_Patios");
        });

        modelBuilder.Entity<Patios>(entity =>
        {
            entity.HasKey(e => e.IdPatio).HasName("PK__Patios__18143B1546F6FFF1");

            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(100);
        });

        modelBuilder.Entity<RegistroActividadBuses>(entity =>
        {
            entity.HasKey(e => e.IdRegistroActividad).HasName("PK__Registro__D36153E6A05A271E");

            entity.Property(e => e.EstadoActividadBus).HasMaxLength(50);
            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NumeroRecorrido).HasMaxLength(50);

            entity.HasOne(d => d.IdPatenteNavigation).WithMany(p => p.RegistroActividadBuses)
                .HasForeignKey(d => d.IdPatente)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registro_Flota");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany(p => p.RegistroActividadBuses)
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Registro_Usuarios");
        });

        modelBuilder.Entity<Roles>(entity =>
        {
            entity.HasKey(e => e.IdRol).HasName("PK__Roles__2A49584C707747F3");

            entity.HasIndex(e => e.Nombre, "UQ__Roles__75E3EFCFC5EA74A6").IsUnique();

            entity.Property(e => e.Descripcion).HasMaxLength(255);
            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<TipoVehiculo>(entity =>
        {
            entity.HasKey(e => e.IdTipoVehiculo).HasName("PK__TipoVehi__DC20741EA38B4636");

            entity.Property(e => e.ImagenUrl).HasMaxLength(255);
            entity.Property(e => e.LargoPx).HasDefaultValue(60);
            entity.Property(e => e.Nombre).HasMaxLength(50);
        });

        modelBuilder.Entity<Usuarios>(entity =>
        {
            entity.HasKey(e => e.IdUsuario).HasName("PK__Usuarios__5B65BF9716008386");

            entity.HasIndex(e => e.Email, "UQ__Usuarios__A9D10534C3F2D943").IsUnique();

            entity.HasIndex(e => e.Rut, "UQ__Usuarios__CAF0366093F6ADDB").IsUnique();

            entity.Property(e => e.Activo).HasDefaultValue(true);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.FechaReg).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.NombreCompleto).HasMaxLength(150);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.PasswordTemp).HasMaxLength(255);
            entity.Property(e => e.Rut).HasMaxLength(12);

            entity.HasOne(d => d.IdRolNavigation).WithMany(p => p.Usuarios)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Usuarios_Roles");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
