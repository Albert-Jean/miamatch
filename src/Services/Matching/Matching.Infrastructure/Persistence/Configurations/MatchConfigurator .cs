using System;
using System.Collections.Generic;
using System.Text;
using Matching.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matching.Infrastructure.Persistence.Configurations
{
    public class MatchConfigurator: IEntityTypeConfiguration<Match>
    {
        public void Configure(EntityTypeBuilder<Match> builder)
        {
            builder.ToTable("matches");
            builder.HasKey(m => m.Id);
            builder.Property(m => m.HouseholdId).HasColumnName("household_id").IsRequired();
            builder.Property(m => m.RecipeId).HasColumnName("recipe_id").IsRequired();
            builder.Property(m => m.DeckId).HasColumnName("deck_id").IsRequired();
            builder.Property(m => m.MatchedAt).HasColumnName("matched_at").IsRequired();
            builder.HasIndex(m => new { m.HouseholdId, m.RecipeId, m.DeckId }).IsUnique();
        }
    }
}
