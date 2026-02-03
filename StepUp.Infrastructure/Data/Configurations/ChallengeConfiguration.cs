using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepUp.Domain.Entities;

namespace StepUp.Infrastructure.Data.Configurations;

public class ChallengeConfiguration : BaseEntityConfiguration<Challenge>
{
    public override void Configure(EntityTypeBuilder<Challenge> builder)
    {
        base.Configure(builder);

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(c => c.MetricType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.StartDate)
            .IsRequired();

        builder.Property(c => c.EndDate)
            .IsRequired();

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(c => c.TargetValue)
            .HasColumnType("decimal(18,2)")
            .IsRequired(false);

        builder.Property(c => c.ExerciseType)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(c => c.Description)
            .HasMaxLength(1000)
            .IsRequired(false);

        builder.Property(c => c.IsSponsored)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(c => c.Prize)
            .HasMaxLength(500)
            .IsRequired(false);

        builder.Property(c => c.SponsorId)
            .IsRequired(false);

        builder.HasOne(c => c.Sponsor)
            .WithMany()
            .HasForeignKey(c => c.SponsorId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.CreatedByUserId)
            .IsRequired(false);

        builder.HasOne(c => c.CreatedByUser)
            .WithMany()
            .HasForeignKey(c => c.CreatedByUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.CompletedAt)
            .IsRequired(false);

        builder.Property(c => c.WinnerUserId)
            .IsRequired(false);

        builder.HasOne(c => c.WinnerUser)
            .WithMany()
            .HasForeignKey(c => c.WinnerUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(c => c.Participations)
            .WithOne(p => p.Challenge)
            .HasForeignKey(p => p.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

