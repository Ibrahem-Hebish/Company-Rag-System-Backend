using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrustructure.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(100);
        builder.Property(c => c.Email).IsRequired().HasMaxLength(100);
        builder.Property(c => c.PhoneNumber).HasMaxLength(20);
        builder.OwnsOne(c => c.Address, a =>
        {
            a.Property(ad => ad.Street).IsRequired().HasMaxLength(100);
            a.Property(ad => ad.City).IsRequired().HasMaxLength(50);
            a.Property(ad => ad.Country).IsRequired().HasMaxLength(50);
        });

        builder.HasIndex(c => c.OwnerId);

        builder.HasMany(c => c.Departments)
               .WithOne(d => d.Company)
               .HasForeignKey(d => d.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Employees)
               .WithOne(d => d.Company)
               .HasForeignKey(d => d.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Documents)
               .WithOne(d => d.Company)
               .HasForeignKey(d => d.CompanyId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Manager)
               .WithOne()
               .HasForeignKey<Company>(d => d.ManagerId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Manager)
               .WithOne()
               .HasForeignKey<Company>(d => d.ManagerId)
               .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("Companies");
    }
}
