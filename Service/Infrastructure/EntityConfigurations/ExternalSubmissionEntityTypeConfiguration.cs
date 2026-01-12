using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Staging.Domain.AggregatesModel.ExternalSubmissionAggregate;

namespace Staging.Infrastructure;

public class ExternalSubmissionEntityTypeConfiguration : IEntityTypeConfiguration<ExternalSubmissionAggregate>
{
    public void Configure(EntityTypeBuilder<ExternalSubmissionAggregate> configuration)
    {
        configuration.ToTable("ExternalSubmission");
        configuration.HasKey(e => e.Id);

        configuration.Property(e => e.SourceType)
            .IsRequired()
            .HasMaxLength(100);

        configuration.Property(e => e.SourceId);

        configuration.OwnsMany(
            e => e.Submissions,
            sb =>
            {
                sb.ToTable("ExternalSubmissionItems");
                sb.WithOwner()
                    .HasForeignKey("ExternalSubmissionId");
                sb.HasKey(s => s.Id);

                sb.Property(s => s.Id)
                    .ValueGeneratedOnAdd();
                
                sb.Property(s => s.Vendor)
                    .IsRequired()
                    .HasMaxLength(50);
                
                sb.Property(s => s.Payload)
                    .IsRequired();

                sb.Property(s => s.Status)
                    .IsRequired()
                    .HasMaxLength(20);

                sb.Property(s => s.ExternalId)
                    .HasMaxLength(100);

                sb.Property(s => s.Message)
                    .HasMaxLength(2000);

                sb.Property(s => s.AttemptCount);

                sb.Property(s => s.CreatedAt)
                    .IsRequired();

                sb.Property(s => s.LastAttemptAt);

                sb.Property(x => x.RequestKey)
                 .HasMaxLength(200)
                 .IsRequired();

                sb.HasIndex(x => new { x.Vendor, x.Type, x.RequestKey })
                 .IsUnique();
            });
    }
}