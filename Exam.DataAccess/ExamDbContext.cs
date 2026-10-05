using Exam.Domain.Enums;
using Exam.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Exam.DataAccess
{
    public class ExamDbContext : DbContext
    {
        public ExamDbContext(DbContextOptions<ExamDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Venue> Venues { get; set; }
        public DbSet<InvigilatorAvailability> InvigilatorAvailabilities { get; set; }
        public DbSet<Allocation> Allocations { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // USERS
            modelBuilder.Entity<User>(entity =>
            {
                entity.ToTable("Users");
                entity.HasKey(u => u.Id);

                entity.Property(u => u.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(u => u.Email)
                      .IsRequired()
                      .HasMaxLength(150);

                entity.HasIndex(u => u.Email).IsUnique();

                entity.Property(u => u.Password)  
                      .IsRequired();

                entity.Property(u => u.ProfileImage)
                      .HasMaxLength(255);

                entity.Property(u => u.ContactNumber)
                      .IsRequired()
                      .HasMaxLength(20);

                entity.Property(u => u.Role)
                      .HasConversion<int>() // Enum to int
                      .IsRequired();

                entity.Property(u => u.Active)
                      .HasDefaultValue(true);

                entity.Property(u => u.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // VENUES
            modelBuilder.Entity<Venue>(entity =>
            {
                entity.ToTable("Venues");
                entity.HasKey(v => v.Id);

                entity.Property(v => v.Name)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(v => v.HallNumber)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.Property(v => v.Capacity)
                      .IsRequired();

                entity.Property(v => v.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");
            });

            // INVIGILATOR AVAILABILITIES
            modelBuilder.Entity<InvigilatorAvailability>(entity =>
            {
                entity.ToTable("InvigilatorAvailabilities");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.InvigilatorId)
                      .IsRequired();

                entity.Property(a => a.DayOfWeek)
                      .HasConversion<int>() // Enum to int
                      .IsRequired();

                entity.Property(a => a.Date)
                  .IsRequired()
                  .HasColumnType("date");

                entity.HasIndex(a => new { a.InvigilatorId, a.DayOfWeek }).IsUnique();

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasOne(a => a.Invigilator)
                  .WithMany() 
                  .HasForeignKey(a => a.InvigilatorId)
                  .OnDelete(DeleteBehavior.Cascade);
            });

            // ALLOCATIONS
            modelBuilder.Entity<Allocation>(entity =>
            {
                entity.ToTable("Allocations");
                entity.HasKey(a => a.Id);

                entity.Property(a => a.VenueId).IsRequired();
                entity.Property(a => a.InvigilatorId).IsRequired();
                entity.Property(a => a.Date).IsRequired();
                entity.Property(a => a.Status).HasConversion<int>().IsRequired();

                entity.Property(a => a.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(a => new { a.VenueId, a.Date }).IsUnique();
                entity.HasIndex(a => new { a.InvigilatorId, a.Date }).IsUnique();

                entity.HasOne(a => a.Venue)
                      .WithMany()
                      .HasForeignKey(a => a.VenueId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(a => a.Invigilator)
                      .WithMany()
                      .HasForeignKey(a => a.InvigilatorId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // NOTIFICATIONS
            modelBuilder.Entity<Notification>(entity =>
            {
                entity.ToTable("Notifications");

                entity.HasKey(n => n.Id);

                entity.Property(n => n.UserId)
                      .IsRequired();

                entity.Property(n => n.Message)
                      .IsRequired()
                      .HasMaxLength(500); 

                entity.Property(n => n.CreatedAt)
                      .HasDefaultValueSql("GETUTCDATE()");

                entity.HasIndex(n => n.UserId); 

                entity.HasOne(n => n.User)
                      .WithMany(u => u.Notifications) 
                      .HasForeignKey(n => n.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });


        }
    }
}
