using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RealtorApi.Domain.Properties;

namespace RealtorApi.Infrastructure.Persistence.Configurations;

public sealed class PropertyConfiguration : IEntityTypeConfiguration<Property>
{
    public void Configure(EntityTypeBuilder<Property> builder)
    {
        builder.ToTable("properties");

        builder.HasKey(property => property.Id);

        builder.Property(property => property.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(property => property.Description)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(property => property.Address)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(property => property.Price)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(property => property.Status)
            .HasConversion<string>()
            .HasColumnType("text")
            .IsRequired();

        builder.Property(property => property.BedroomCount)
            .IsRequired();

        builder.Property(property => property.BathroomCount)
            .IsRequired();

        builder.Property(property => property.AreaSquareMeters)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(property => property.ImageUrl)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(property => property.CreatedAt)
            .HasDefaultValueSql("CURRENT_TIMESTAMP")
            .IsRequired();

        builder.Property(property => property.UpdatedAt)
            .IsRequired(false);

        builder.HasIndex(property => property.Status);
    }
}
