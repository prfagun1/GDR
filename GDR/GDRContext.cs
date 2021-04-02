using GDR.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GDR
{
    public class GDRContext : DbContext
    {
        public GDRContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Ldap>().HasKey(t => t.Id);
            modelBuilder.Entity<PermissionGroup>().HasKey(t => t.Id);
            modelBuilder.Entity<Log>().HasKey(t => t.Id);
            modelBuilder.Entity<Parameter>().HasKey(t => t.Id);

            modelBuilder.Entity<DatabaseGDR>()
                .HasKey(t => t.Id);
            //modelBuilder.Entity<DatabaseGDR>()
                //.HasOne(t => t.DatabaseType)
                //.WithMany( t=> t.DatabaseGDR)
                //.HasForeignKey<DatabaseGDR>(b => b.DatabaseTypeId);

            modelBuilder.Entity<DatabaseType>().HasKey(t => t.Id);
            modelBuilder.Entity<Report>().HasKey(t => t.Id);

            modelBuilder.Entity<ReportDatabaseGDR>()
                .HasKey(t => new { t.ReportID, t.DatabaseGDRId });
            modelBuilder.Entity<ReportDatabaseGDR>()
                .HasOne(t => t.Report)
                .WithMany(t => t.ReportDatabaseGDR)
                .HasForeignKey(t => t.ReportID);
            modelBuilder.Entity<ReportDatabaseGDR>()
                .HasOne(t => t.DatabaseGDR)
                .WithMany(t => t.ReportDatabaseGDR)
                .HasForeignKey(t => t.DatabaseGDRId);

            modelBuilder.Entity<ReportPermissionGroup>()
                .HasKey(t => new { t.ReportID, t.PermissionGroupId });
            modelBuilder.Entity<ReportPermissionGroup>()
                .HasOne(t => t.Report)
                .WithMany(t => t.ReportPermissionGroup)
                .HasForeignKey(t => t.ReportID);
            modelBuilder.Entity<ReportPermissionGroup>()
                .HasOne(t => t.PermissionGroup)
                .WithMany(t => t.ReportPermissionGroup)
                .HasForeignKey(t => t.PermissionGroupId);
        }

        //public DbSet<GDR.Models.DatabaseGDR> DatabaseGDR { get; set; }
        //public DbSet<GDR.Models.PermissionGroup> PermissionGroup { get; set; }
        //public DbSet<GDR.Models.Report> Report { get; set; }
        //public DbSet<GDR.Models.DatabaseType> DatabaseType { get; set; }
        public DbSet<GDR.Models.ReportDatabaseGDR> ReportDatabaseGDR { get; set; }
        public DbSet<GDR.Models.ReportPermissionGroup> ReportPermissionGroup { get; set; }
        public DbSet<GDR.Models.Parameter> Parameter { get; set; }


    }
}

