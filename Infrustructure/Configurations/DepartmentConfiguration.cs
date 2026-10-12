using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrustructure.Configurations;

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(d => d.CompanyId);

        builder.Property(d => d.Name).IsRequired().HasMaxLength(100);

        builder.HasMany(d => d.Employees)
               .WithOne(e => e.Department)
               .HasForeignKey(e => e.DepartmentId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Manager)
              .WithOne()
              .HasForeignKey<Department>(d => d.ManagerId)
              .OnDelete(DeleteBehavior.NoAction);

        builder.HasMany(d => d.Documents)
               .WithOne(doc => doc.Department)
               .HasForeignKey(doc => doc.DepartmentId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("Departments");
    }
}
