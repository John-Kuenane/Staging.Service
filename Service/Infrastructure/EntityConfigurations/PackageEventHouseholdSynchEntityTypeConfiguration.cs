using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

class PackageEventHouseholdSynchEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHouseholdSynch>
{
    public void Configure(EntityTypeBuilder<PackageEventHouseholdSynch> configuration)
    {
        configuration.ToTable("PackageEventHouseholdSynch");

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

        configuration.HasMany(b => b.MetaAttributes)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdSynchId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHouseholdSynch.MetaAttributes));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
