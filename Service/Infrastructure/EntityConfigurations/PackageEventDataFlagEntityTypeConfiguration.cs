using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

class PackageEventDataFlagEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventDataFlag>
{
    public void Configure(EntityTypeBuilder<PackageEventDataFlag> configuration)
    {
        configuration.ToTable("PackageEventDataFlag");

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
            .Property(c => c.HouseholdId)
            .IsRequired();

        configuration
            .Property(c => c.DataFlagTypeId)
            .IsRequired();

        configuration
            .OwnsOne(o => o.FlagResolved, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.FlagDeferred, a =>
            {
                a.WithOwner();
            });

        configuration
            .Property(c => c.Description)
            .IsRequired()
            .HasMaxLength(500);

        configuration.HasMany(b => b.Comments)
           .WithOne()
           .HasForeignKey("PackageEventDataFlagId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventDataFlag.Comments));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
