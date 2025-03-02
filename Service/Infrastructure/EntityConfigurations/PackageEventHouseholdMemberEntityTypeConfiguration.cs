using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MISSA.Services.Staging.Domain.AggregatesModel.PackageAggregate;

namespace MISSA.Services.Staging.Infrastructure;

class PackageEventHouseholdMemberEntityTypeConfiguration : IEntityTypeConfiguration<PackageEventHouseholdMember>
{
    public void Configure(EntityTypeBuilder<PackageEventHouseholdMember> configuration)
    {
        configuration.ToTable("PackageEventHouseholdMember");

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
            .Property(c => c.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        configuration
            .Property(c => c.Surname)
            .IsRequired()
            .HasMaxLength(100);

        configuration
            .Property(c => c.IDDocumentType)
            .HasMaxLength(50);

        configuration
            .Property(c => c.IdentificationNumber)
            .HasMaxLength(50);

        configuration.HasMany(b => b.Attributes)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdMemberId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHouseholdMember.Attributes));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
