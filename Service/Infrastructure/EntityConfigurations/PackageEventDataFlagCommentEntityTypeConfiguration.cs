using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventDataFlagCommentEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventDataFlagComment>
{
    public void Configure(EntityTypeBuilder<PackageEventDataFlagComment> configuration)
    {
        configuration.ToTable("PackageEventDataFlagComment");

        configuration.Ignore(b => b.DomainEvents);

        configuration
            .Property<DateTime?>("Created");
        configuration
            .Property<Guid?>("CreatedId");
        configuration
            .Property<DateTime?>("LastModified");
        configuration
            .Property<Guid?>("LastModifiedId");

        configuration
            .Property(c => c.DeviceId)
            .HasMaxLength(10);

        configuration
            .Property(c => c.Comment)
            .IsRequired()
            .HasMaxLength(500);
    }
}
