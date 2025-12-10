using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace ExamBurcu.Data;

public partial class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<child> children { get; set; }

    public virtual DbSet<doctor> doctors { get; set; }

    public virtual DbSet<vaccine> vaccines { get; set; }

    public virtual DbSet<vaccineapplication> vaccineapplications { get; set; }

    public virtual DbSet<vaccineschedule> vaccineschedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<child>(entity =>
        {
            entity.HasKey(e => e.id).HasName("child_pk");

            entity.ToTable("child");

            entity.HasIndex(e => e.tckn, "child_unique").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.namesurname).HasColumnType("character varying");
        });

        modelBuilder.Entity<doctor>(entity =>
        {
            entity.HasKey(e => e.id).HasName("doctor_pk");

            entity.ToTable("doctor");

            entity.HasIndex(e => e.tckn, "doctor_unique").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.namesurname).HasColumnType("character varying");
        });

        modelBuilder.Entity<vaccine>(entity =>
        {
            entity.HasKey(e => e.id).HasName("vaccine_pk");

            entity.ToTable("vaccine");

            entity.HasIndex(e => e.code, "vaccine_unique").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.code).HasColumnType("character varying");
            entity.Property(e => e.name).HasColumnType("character varying");
        });

        modelBuilder.Entity<vaccineapplication>(entity =>
        {
            entity.HasKey(e => e.id).HasName("vaccineapplication_pk");

            entity.ToTable("vaccineapplication");

            entity.Property(e => e.id).UseIdentityAlwaysColumn();
            entity.Property(e => e.description).HasColumnType("character varying");

            entity.HasOne(d => d.child).WithMany(p => p.vaccineapplications)
                .HasForeignKey(d => d.childid)
                .HasConstraintName("vaccineapplication_child_fk");

            entity.HasOne(d => d.doctor).WithMany(p => p.vaccineapplications)
                .HasForeignKey(d => d.doctorid)
                .HasConstraintName("vaccineapplication_doctor_fk");

            entity.HasOne(d => d.vaccine).WithMany(p => p.vaccineapplications)
                .HasForeignKey(d => d.vaccineid)
                .HasConstraintName("vaccineapplication_vaccine_fk");
        });

        modelBuilder.Entity<vaccineschedule>(entity =>
        {
            entity.HasKey(e => e.id).HasName("vaccineschedule_pk");

            entity.ToTable("vaccineschedule");

            entity.HasIndex(e => new { e.vaccineid, e.month }, "vaccineschedule_unique").IsUnique();

            entity.Property(e => e.id).UseIdentityAlwaysColumn();

            entity.HasOne(d => d.vaccine).WithMany(p => p.vaccineschedules)
                .HasForeignKey(d => d.vaccineid)
                .HasConstraintName("vaccineschedule_vaccine_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
