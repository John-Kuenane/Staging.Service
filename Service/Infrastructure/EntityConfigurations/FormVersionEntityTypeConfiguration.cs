using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormVersionEntityTypeConfiguration : IEntityTypeConfiguration<FormVersion>
{
    public void Configure(EntityTypeBuilder<FormVersion> configuration)
    {
        configuration.ToTable("FormVersion");

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

        configuration
            .OwnsOne(o => o.Version, a =>
            {
                a.WithOwner();
            });
    }
}