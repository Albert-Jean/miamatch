using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShoppingList.Domain.Entities;

namespace ShoppingList.Infrastructure.Persistence.Configurations
{
    public class ShoppingListItemConfiguration: IEntityTypeConfiguration<ShoppingListItem>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<ShoppingListItem> builder)
        {
            builder.ToTable("shopping_list_items");
            builder.HasKey(sli => sli.Id);
            builder.Property(sli => sli.HouseholdId).HasColumnName("household_id").IsRequired();
            builder.Property(sli => sli.IngredientName).HasColumnName("ingredient_name").IsRequired();
            builder.Property(sli => sli.Measure).HasColumnName("measure").IsRequired();
            builder.Property(sli => sli.AddedAt).HasColumnName("added_at").IsRequired();
            builder.Property(sli => sli.RecipeId).HasColumnName("recipe_id").IsRequired();
        }
    }
}
