using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepUp.Domain.Entities;

namespace StepUp.Infrastructure.Data.Configurations;

public class ParticipationConfiguration : BaseEntityConfiguration<Participation>
{
    public override void Configure(EntityTypeBuilder<Participation> builder)
    {
        base.Configure(builder);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.ChallengeId)
            .IsRequired();

        builder.Property(p => p.TotalScore)
            .IsRequired()
            .HasPrecision(18, 2);

        // Composite unique index to prevent duplicate participations
        builder.HasIndex(p => new { p.UserId, p.ChallengeId })
            .IsUnique();

        builder.HasOne(p => p.User)
            .WithMany(u => u.Participations)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Challenge)
            .WithMany(c => c.Participations)
            .HasForeignKey(p => p.ChallengeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

