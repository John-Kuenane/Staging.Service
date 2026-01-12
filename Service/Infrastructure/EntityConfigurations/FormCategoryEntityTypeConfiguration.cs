using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormCategoryEntityTypeConfiguration : IEntityTypeConfiguration<FormCategory>
{
    public void Configure(EntityTypeBuilder<FormCategory> configuration)
    {
        configuration.ToTable("FormCategory");

        configuration.HasKey(e => e.Id);

        configuration.Ignore(b => b.DomainEvents);

        configuration.Property<int>("FormId")
            .IsRequired();

        configuration
            .Property<DateTime?>("Created");
        configuration
            .Property<Guid?>("CreatedId");
        configuration
            .Property<DateTime?>("LastModified");
        configuration
            .Property<Guid?>("LastModifiedId");

        configuration.Property(b => b.ShortName)
            .HasMaxLength(100)
            .IsRequired();

        configuration.HasIndex("ShortName")
            .IsUnique(false);

        configuration.Property(b => b.FriendlyName)
            .HasMaxLength(250)
            .IsRequired();

        configuration.Property(b => b.Help)
            .HasMaxLength(500);

        configuration.Property(b => b.Order)
            .IsRequired();

        configuration
            .OwnsOne(o => o.VersionCreated, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.VersionModified, a =>
            {
                a.WithOwner();
            });

        configuration
            .OwnsOne(o => o.VersionDeleted, a =>
            {
                a.WithOwner();
            });

        configuration.Property(b => b.ExtendableTypeName)
            .HasMaxLength(30);

        configuration.HasMany(b => b.Elements)
           .WithOne()
           .HasForeignKey("FormCategoryId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(FormCategory.Elements));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}