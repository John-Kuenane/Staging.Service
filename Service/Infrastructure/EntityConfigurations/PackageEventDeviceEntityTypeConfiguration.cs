using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

class PackageEventDeviceEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventDevice>
{
    public void Configure(EntityTypeBuilder<PackageEventDevice> configuration)
    {
        configuration.ToTable("PackageEventDevice");

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
            .IsRequired()
            .HasMaxLength(10);

        configuration
            .Property(c => c.EnumeratorName)
            .HasMaxLength(75);
    }
}
