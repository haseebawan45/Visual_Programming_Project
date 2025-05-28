using AuthWebApp.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthWebApp.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }
        
        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseRegistration> CourseRegistrations { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<FeeRecord> FeeRecords { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            // Configure relationships and constraints
            builder.Entity<CourseRegistration>()
                .HasOne(cr => cr.Student)
                .WithMany(s => s.CourseRegistrations)
                .HasForeignKey(cr => cr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<CourseRegistration>()
                .HasOne(cr => cr.Course)
                .WithMany(c => c.Registrations)
                .HasForeignKey(cr => cr.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<Assignment>()
                .HasOne(a => a.Student)
                .WithMany(s => s.Assignments)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.NoAction); // Don't delete assignments when student is deleted
                
            builder.Entity<Assignment>()
                .HasOne(a => a.Course)
                .WithMany(c => c.Assignments)
                .HasForeignKey(a => a.CourseId)
                .OnDelete(DeleteBehavior.Cascade);
                
            builder.Entity<FeeRecord>()
                .HasOne(f => f.Student)
                .WithMany(s => s.FeeRecords)
                .HasForeignKey(f => f.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 