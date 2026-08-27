using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Recipes.Domain.Entities
{
    public class Recipe
    {
        public Guid Id { get; }
        public string ExternalId { get; }
        public string Name { get; }
        public string Instructions { get; }
        public IReadOnlyCollection<RecipeIngredient> Ingredients { get; }
        public DateTime CacheAt { get; }
        public string ImageUrl { get; }
        public string[] Tags { get; }

        private Recipe(Guid id, string externalId, string name, string instructions, IReadOnlyCollection<RecipeIngredient> ingredients, DateTime cacheAt, string imageUrl, string[] tags)
        {
            Id = id;
            ExternalId = externalId;
            Name = name;
            Instructions = instructions;
            Ingredients = ingredients;
            CacheAt = cacheAt;
            ImageUrl = imageUrl;
            Tags = tags;
        }
        private Recipe(Guid id, string externalId, string name, string instructions, DateTime cacheAt, string imageUrl, string[] tags)
        {
            Id = id;
            ExternalId = externalId;
            Name = name;
            Instructions = instructions;
            CacheAt = cacheAt;
            ImageUrl = imageUrl;
            Tags = tags;
            Ingredients = new List<RecipeIngredient>();
        }
        public static Recipe Create(string externalId, string name, string instructions, IReadOnlyCollection<RecipeIngredient> ingredients, string imageUrl, IEnumerable<string>? tags = null)
        {
            Guid id = Guid.NewGuid();
            DateTime cacheAt = DateTime.UtcNow;
            return new Recipe(id, externalId, name, instructions, ingredients, cacheAt, imageUrl, tags?.ToArray() ?? Array.Empty<string>());
        }
    }
}
