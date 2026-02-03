using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StepUp.Domain.Entities;

namespace StepUp.Infrastructure.Data.Configurations;

public class FriendRequestConfiguration : BaseEntityConfiguration<FriendRequest>
{
    public override void Configure(EntityTypeBuilder<FriendRequest> builder)
    {
        base.Configure(builder);

        builder.Property(fr => fr.FromUserId)
            .IsRequired();

        builder.Property(fr => fr.ToUserId)
            .IsRequired();

        builder.Property(fr => fr.Status)
            .IsRequired()
            .HasConversion<int>();

        // Relație către FromUser
        builder.HasOne(fr => fr.FromUser)
            .WithMany()
            .HasForeignKey(fr => fr.FromUserId)
            .OnDelete(DeleteBehavior.Restrict); // Nu șterge request-urile când user-ul este șters

        // Relație către ToUser
        builder.HasOne(fr => fr.ToUser)
            .WithMany()
            .HasForeignKey(fr => fr.ToUserId)
            .OnDelete(DeleteBehavior.Restrict); // Nu șterge request-urile când user-ul este șters

        // Index unic pentru a preveni duplicate requests între aceiași useri
        builder.HasIndex(fr => new { fr.FromUserId, fr.ToUserId })
            .IsUnique();

        // Index pentru a accelera căutările după status
        builder.HasIndex(fr => fr.Status);
    }
}

