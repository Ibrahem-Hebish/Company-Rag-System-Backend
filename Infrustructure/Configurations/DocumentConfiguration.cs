using Domain.Entities;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Document = Domain.Entities.Document;

namespace Infrustructure.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.HasIndex(e => e.DepartmentId);

        builder.Property(d => d.Path).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Name).IsRequired();
        builder.Property(d => d.UploadedAt).IsRequired();

        builder.HasOne(d => d.Uploader)
            .WithMany(e => e.UploadedDocuments)
            .HasForeignKey(d => d.UploaderId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(d => d.Modifier)
            .WithMany(e => e.ModifiedDocuments)
            .HasForeignKey(d => d.ModifierId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.ToTable("Documents");
    }
}