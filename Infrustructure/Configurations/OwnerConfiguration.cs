using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrustructure.Configurations;

internal class OwnerConfiguration : IEntityTypeConfiguration<Owner>
{
    public void Configure(EntityTypeBuilder<Owner> builder)
    {
        builder.HasMany(o => o.Companies)
               .WithOne(c => c.Owner)
               .HasForeignKey(c => c.OwnerId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Owners");
    }
}
