using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Recipe.Domain.Entities;

namespace Recipes.Infrastructure.Persistence.Configurations
{
    public class DeckConfiguration: IEntityTypeConfiguration<Deck>
    {
        public void Configure(EntityTypeBuilder<Deck> builder)
        {
            builder.ToTable("decks");
            builder.HasKey(d => d.Id);
            builder.Property(d => d.HouseholdId).HasColumnName("household_id").IsRequired();
            builder.Property(d => d.GeneratedAt).HasColumnName("generated_at").IsRequired();
            builder.PrimitiveCollection(d => d.RecipeIds)
                    .HasColumnName("recipe_ids")
                    .IsRequired()
                    .HasField("_recipeIds");

        }
    }
}
