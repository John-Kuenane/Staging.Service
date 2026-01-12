using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormElementEntityTypeConfiguration : IEntityTypeConfiguration<FormElement>
{
    public void Configure(EntityTypeBuilder<FormElement> configuration)
    {
        configuration.ToTable("FormElement");

        configuration.HasKey(e => e.Id);

        configuration.Ignore(b => b.DomainEvents);

        configuration.Property<int>("FormCategoryId")
            .IsRequired();

        configuration
            .Property<DateTime?>("Created");
        configuration
            .Property<Guid?>("CreatedId");
        configuration
            .Property<DateTime?>("LastModified");
        configuration
            .Property<Guid?>("LastModifiedId");

        configuration
            .Property(c => c.ElementTypeId)
            .IsRequired();

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

        configuration.Property(b => b.Category)
            .HasMaxLength(75);

        configuration.Property(b => b.AttributeKey)
            .HasMaxLength(150);

        configuration.Property(b => b.AttributeCode)
            .HasMaxLength(10);

        configuration.Property(b => b.RegEx)
            .HasMaxLength(150);

        configuration.HasMany(b => b.Attributes)
           .WithOne()
           .HasForeignKey("FormElementId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(FormElement.Attributes));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Dependencies)
            .WithOne()
            .HasForeignKey("FormElementId")
            .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(FormElement.Dependencies));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}