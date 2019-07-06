//------------------------------------------------------------------------------
// <auto-generated>
//    This code is auto-generated by a T4 template. This class can be only customized modifying the corresponding partial class.
//    All other changes to this file will cause incorrect behavior and will be lost when the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
using AutoEcMvc.Models;

using Microsoft.EntityFrameworkCore;

namespace AutoEcMvc.Data
{
    public partial class MetadataContext : DbContext
    {
        public MetadataContext(DbContextOptions<MetadataContext> options) : base(options)
        {
        }

        public DbSet<Course> Courses { get; set; }
        public DbSet<CourseAssignment> CourseAssignments { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Instructor> Instructors { get; set; }
        public DbSet<OfficeAssignment> OfficeAssignments { get; set; }
        public DbSet<Person> Persons { get; set; }
        public DbSet<Student> Students { get; set; }        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Course>().ToTable(nameof(Course));
            modelBuilder.Entity<CourseAssignment>().ToTable(nameof(CourseAssignment));
            modelBuilder.Entity<Department>().ToTable(nameof(Department));
            modelBuilder.Entity<Enrollment>().ToTable(nameof(Enrollment));
            modelBuilder.Entity<Instructor>().ToTable(nameof(Instructor));
            modelBuilder.Entity<OfficeAssignment>().ToTable(nameof(OfficeAssignment));
            modelBuilder.Entity<Person>().ToTable(nameof(Person));
            modelBuilder.Entity<Student>().ToTable(nameof(Student));
            _OnModelCreating(modelBuilder);
        }
    }
}
