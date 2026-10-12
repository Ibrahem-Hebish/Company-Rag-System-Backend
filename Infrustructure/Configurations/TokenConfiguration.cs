using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrustructure.Configurations;

public sealed class TokenConfiguration : IEntityTypeConfiguration<Token>
{
    public void Configure(EntityTypeBuilder<Token> builder)
    {
        builder.HasKey(t => t.Id);

        builder.HasIndex(t => t.IsActive);

        builder.HasQueryFilter(t => t.IsActive);

        builder.HasOne(t => t.User)
               .WithMany(u => u.Tokens)
               .HasForeignKey(t => t.UserId)
               .OnDelete(DeleteBehavior.Cascade);
        
        builder.ToTable("Tokens");
    }
}
