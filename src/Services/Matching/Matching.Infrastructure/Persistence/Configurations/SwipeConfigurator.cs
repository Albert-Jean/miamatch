using System;
using System.Collections.Generic;
using System.Text;
using Matching.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matching.Infrastructure.Persistence.Configurations
{
    public class SwipeConfigurator: IEntityTypeConfiguration<Swipe>
    {
        public void Configure(EntityTypeBuilder<Swipe> builder)
        {
            builder.ToTable("swipes");
            builder.HasKey(s => s.Id);
            builder.Property(s => s.UserId).HasColumnName("user_id").IsRequired();
            builder.Property(s => s.HouseholdId).HasColumnName("household_id").IsRequired();
            builder.Property(s => s.RecipeId).HasColumnName("recipe_id").IsRequired();
            builder.Property(s => s.DeckId).HasColumnName("deck_id").IsRequired();
            builder.Property(s => s.Liked).HasColumnName("liked").IsRequired();
            builder.Property(s => s.SwipedAt).HasColumnName("swiped_at").IsRequired();
            builder.HasIndex(s => new { s.UserId, s.HouseholdId, s.RecipeId, s.DeckId }).IsUnique();
        }
    }
}
