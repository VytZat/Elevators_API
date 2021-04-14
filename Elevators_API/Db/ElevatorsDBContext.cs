using System.Reflection;
using Microsoft.EntityFrameworkCore;

namespace Elevators_API.Model.DbModel
{
    public partial class ElevatorsDBContext : DbContext
    {
        public ElevatorsDBContext()
        {
        }

        public ElevatorsDBContext(DbContextOptions<ElevatorsDBContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Elevators> Elevators { get; set; }
        public virtual DbSet<Logs> Logs { get; set; }
        public virtual DbSet<Statuses> Statuses { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlite("FileName=ElevatorsDatabase.db", options => 
                {
                    options.MigrationsAssembly(Assembly.GetExecutingAssembly().FullName);
                });
            }
            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Elevators>(entity =>
            {
                entity.Property(e => e.StatusId).HasColumnName("Status_id");

                entity.HasOne(d => d.Status)
                    .WithMany(p => p.Elevators)
                    .HasForeignKey(d => d.StatusId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Elevators_Statuses");
            });

            modelBuilder.Entity<Logs>(entity =>
            {
                entity.Property(e => e.Description).IsRequired();

                entity.Property(e => e.Time).HasColumnType("datetime");
            });

            modelBuilder.Entity<Statuses>(entity =>
            {
                entity.Property(e => e.Status)
                    .IsRequired()
                    .HasMaxLength(50);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}
