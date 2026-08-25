using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Recipes.Infrastructure.Persistence.Configurations
{
    public class RecipeConfiguration: IEntityTypeConfiguration<Recipes.Domain.Entities.Recipe>
    {
        public void Configure(EntityTypeBuilder<Recipes.Domain.Entities.Recipe> builder)
        {
            builder.ToTable("recipes");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.MealDbId).HasColumnName("meal_db_id").IsRequired();
            builder.Property(r => r.Name).HasColumnName("name").IsRequired();
            builder.Property(r => r.Instructions).HasColumnName("instructions").IsRequired();
            builder.Property(r => r.CacheAt).HasColumnName("cache_at").IsRequired();
            builder.Property(r => r.ImageUrl).HasColumnName("image_url").IsRequired();
            builder.OwnsMany(r => r.Ingredients, ib =>
            {
                ib.ToJson();
            });
            builder.HasIndex(r => r.MealDbId).IsUnique();
            builder.Navigation(r => r.Ingredients)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

        }
    }
}
