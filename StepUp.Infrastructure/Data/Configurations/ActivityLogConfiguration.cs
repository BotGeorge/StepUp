using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepUp.Domain.Entities;

namespace StepUp.Infrastructure.Data.Configurations;

public class ActivityLogConfiguration : BaseEntityConfiguration<ActivityLog>
{
    public override void Configure(EntityTypeBuilder<ActivityLog> builder)
    {
        base.Configure(builder);

        builder.Property(a => a.UserId)
            .IsRequired();

        builder.Property(a => a.Date)
            .IsRequired();

        builder.Property(a => a.MetricValue)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(a => a.ExerciseType)
            .HasMaxLength(50);

        builder.HasOne(a => a.ParentActivityLog)
            .WithMany(a => a.GeneratedLogs)
            .HasForeignKey(a => a.ParentActivityLogId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index for efficient queries by user and date
        builder.HasIndex(a => new { a.UserId, a.Date });
        builder.HasIndex(a => a.ParentActivityLogId);
    }
}

