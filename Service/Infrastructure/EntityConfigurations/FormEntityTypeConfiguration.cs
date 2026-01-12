using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormEntityTypeConfiguration : IEntityTypeConfiguration<Form>
{
    public void Configure(EntityTypeBuilder<Form> configuration)
    {
        configuration.ToTable("Form");

        configuration.HasKey(e => e.Id);

        configuration.Ignore(b => b.DomainEvents);

        configuration
            .Property<DateTime?>("Created");
        configuration
            .Property<Guid?>("CreatedId");
        configuration
            .Property<DateTime?>("LastModified");
        configuration
            .Property<Guid?>("LastModifiedId");

        configuration.Property(b => b.ShortName)
            .HasMaxLength(50)
            .IsRequired();

        configuration.Property(b => b.UniqueCode)
            .HasMaxLength(3)
            .IsRequired();

        configuration.HasIndex("ShortName")
            .IsUnique();

        configuration.Property(b => b.FriendlyName)
            .HasMaxLength(150)
            .IsRequired();

        configuration.Property(b => b.Help)
            .HasMaxLength(500);

        configuration
            .Property(c => c.FormStatusId)
            .IsRequired();

        configuration
            .OwnsOne(o => o.CurrentVersion, a =>
            {
                a.WithOwner();
            });

        configuration.HasMany(b => b.Categories)
           .WithOne()
           .HasForeignKey("FormId")
           .OnDelete(DeleteBehavior.Cascade);

        var navigation = configuration.Metadata.FindNavigation(nameof(Form.Categories));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);

        configuration.HasMany(b => b.Versions)
            .WithOne()
            .HasForeignKey("FormId")
            .OnDelete(DeleteBehavior.Cascade);

        navigation = configuration.Metadata.FindNavigation(nameof(Form.Versions));
        navigation.SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}