using AutoEcMvc.Models;

using Microsoft.EntityFrameworkCore;

namespace AutoEcMvc.Data
{
    public partial class MetadataContext
    {
        protected void _OnModelCreating(ModelBuilder modelBuilder) => modelBuilder.Entity<CourseAssignment>()
                .HasKey(c => new { c.CourseID, c.InstructorID });
    }
}
