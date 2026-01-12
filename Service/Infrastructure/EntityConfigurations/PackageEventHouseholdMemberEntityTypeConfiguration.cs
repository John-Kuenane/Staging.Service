using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.PackageAggregate;

namespace Staging.Infrastructure;

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
            .Property(c => c.HouseholdMemberId)
            .IsRequired();

        configuration
            .Property(c => c.HouseholdMemberGuid)
            .IsRequired();

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

        configuration
            .Property(c => c.Gender)
            .HasMaxLength(20);

        configuration.OwnsOne(o => o.EnumeratedIdentity, a =>
        {
            a.Property(p => p.IdentificationNumber).HasMaxLength(50);
            a.Property(p => p.FirstName).HasMaxLength(100);
            a.Property(p => p.Surname).HasMaxLength(100);
            a.Property(p => p.DateOfBirth);
        });

        configuration.OwnsOne(o => o.IdentityVerification, a =>
        {
            a.Property(p => p.Status)
                .HasMaxLength(20)
                .IsRequired()
                .HasDefaultValue(IdentityVerificationStatus.Pending);

            a.Property(p => p.RecordedAt);
            a.Property(p => p.Message).HasMaxLength(500);
            a.Property(p => p.Reference).HasMaxLength(100);

            a.OwnsOne(p => p.VerifiedIdentity, vi =>
            {
                vi.Property(x => x.IdentificationNumber).HasMaxLength(50);
                vi.Property(x => x.FirstName).HasMaxLength(100);
                vi.Property(x => x.Surname).HasMaxLength(100);
                vi.Property(x => x.DateOfBirth);
            });

            a.Navigation(x => x.VerifiedIdentity).IsRequired(false);
        });

        configuration.HasMany(b => b.Attributes)
           .WithOne()
           .HasForeignKey("PackageEventHouseholdMemberId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(PackageEventHouseholdMember.Attributes));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.Navigation(x => x.EnumeratedIdentity).IsRequired(false);
        configuration.Navigation(x => x.IdentityVerification).IsRequired();
    }
}
