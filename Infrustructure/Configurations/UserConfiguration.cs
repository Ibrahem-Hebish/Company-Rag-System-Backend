using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrustructure.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.FirstName).IsRequired().HasMaxLength(50);
        builder.Property(u => u.LastName).IsRequired().HasMaxLength(50);

        builder.Property(u => u.Email).IsRequired().HasMaxLength(100);
        builder.Property(u => u.HashedPassword).IsRequired();

        builder.Property(u => u.PhoneNumber).HasMaxLength(20);
        builder.OwnsOne(u => u.Address, a =>
        {
            a.Property(ad => ad.Street).IsRequired().HasMaxLength(100);
            a.Property(ad => ad.City).IsRequired().HasMaxLength(50);
            a.Property(ad => ad.Country).IsRequired().HasMaxLength(50);
        });
        
        builder.HasIndex(u => u.Email)
               .IsUnique();

        builder.HasMany(u => u.Tokens)
               .WithOne(t => t.User)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Users");
    }
}