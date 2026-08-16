using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Infrastructure.Persistence.Configurations
{
    public class HouseholdConfiguration : IEntityTypeConfiguration<Household>
    {
        public void Configure(EntityTypeBuilder<Household> builder)
        {
            builder.ToTable("households");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.Name).HasColumnName("name").IsRequired();
            builder.Property(h => h.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(h=> h.InviteCode).HasConversion(code=>code.Value,value=>InviteCode.From(value)).HasColumnName("invite_code").IsRequired();
            builder.HasIndex(h => h.InviteCode).IsUnique();
            builder.OwnsMany(h => h.Members, membersBuilder =>
            {
                membersBuilder.ToTable("household_members");
                membersBuilder.Property<Guid>("HouseholdId");
                membersBuilder.WithOwner().HasForeignKey("HouseholdId");
                membersBuilder.HasKey("HouseholdId", nameof(HouseholdMember.UserId));
                membersBuilder.Property(m => m.UserId).HasColumnName("user_id");
                membersBuilder.Property(m => m.JoinedAt).HasColumnName("joined_at");
            });

            builder.Navigation(h => h.Members)
                .HasField("_members")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        }
    }
}

