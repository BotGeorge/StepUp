using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepUp.Domain.Entities;

namespace StepUp.Infrastructure.Data.Configurations;

public class UserConfiguration : BaseEntityConfiguration<User>
{
    public override void Configure(EntityTypeBuilder<User> builder)
    {
        base.Configure(builder);

        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(255);

         builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(u => u.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(u => u.LastActiveAt)
            .IsRequired(false);

        builder.Property(u => u.ProfileImageUrl)
            .IsRequired(false)
            .HasMaxLength(1000);

        builder.Property(u => u.HeightCm)
            .IsRequired(false);

        builder.Property(u => u.WeightKg)
            .HasColumnType("numeric(5,2)")
            .IsRequired(false);

        builder.Property(u => u.Age)
            .IsRequired(false);

        builder.Property(u => u.Gender)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(u => u.DailyStepsGoal)
            .IsRequired()
            .HasDefaultValue(10000);

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.HasMany(u => u.Participations)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.ActivityLogs)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

