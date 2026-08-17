using System;
using System.Collections.Generic;
using System.Text;

namespace Recipes.Domain.Entities
{
    public class Recipe
    {
        public Guid Id { get; }
        public string MealDbId { get; }
        public string Name { get; }
        public string Instructions { get; }
        public IReadOnlyCollection<RecipeIngredient> Ingredients { get; }
        public DateTime CacheAt { get; }
        public string ImageUrl { get; }

        private Recipe (Guid id, string mealDbId, string name, string instructions, IReadOnlyCollection<RecipeIngredient> ingredients, DateTime cacheAt, string imageUrl)
        {
            Id = id;
            MealDbId = mealDbId;
            Name = name;
            Instructions = instructions;
            Ingredients = ingredients;
            CacheAt = cacheAt;
            ImageUrl = imageUrl;
        }
        public static Recipe Create(string mealDbId, string name, string instructions, IReadOnlyCollection<RecipeIngredient> ingredients, string imageUrl)
        {
            Guid id = Guid.NewGuid();
            DateTime cacheAt = DateTime.UtcNow;
            return new Recipe(id, mealDbId, name, instructions, ingredients, cacheAt, imageUrl);
        }
    }
}
