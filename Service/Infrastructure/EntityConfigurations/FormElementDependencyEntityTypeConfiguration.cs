using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.FormAggregate;

namespace Staging.Infrastructure;

class FormElementDependencyEntityTypeConfiguration : IEntityTypeConfiguration<FormElementDependency>
{
    public void Configure(EntityTypeBuilder<FormElementDependency> configuration)
    {
        configuration.ToTable("FormElementDependency");

        configuration.HasKey(e => e.Id);

        configuration.Ignore(b => b.DomainEvents);

        configuration.Property<int>("FormElementId")
            .IsRequired();

        configuration.Property<Guid>("ComparisonFormElementId")
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
            .Property(c => c.DependencyRelationshipTypeId)
            .IsRequired();

        configuration
            .Property(c => c.OperatorTypeId)
            .IsRequired();

        configuration.Property(c => c.ComparisonValue)
            .IsRequired();
    }
}