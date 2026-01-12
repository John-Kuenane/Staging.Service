using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormElementAttributeEntityTypeConfiguration : IEntityTypeConfiguration<FormElementAttribute>
{
    public void Configure(EntityTypeBuilder<FormElementAttribute> configuration)
    {
        configuration.ToTable("FormElementAttribute");

        configuration.HasKey(e => e.Id);

        configuration.Ignore(b => b.DomainEvents);

        configuration.Property<int>("FormElementId")
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
            .Property(c => c.AttributeTypeId)
            .IsRequired();
    }
}