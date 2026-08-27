using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Recipes.Application.Abstractions;

namespace Recipes.Infrastructure.Catalog
{
    public class SeedRecipeCatalog : IRecipeCatalog
    {
        private const string ResourceName = "Recipes.Infrastructure.SeedData.recipes.json";
        private static readonly Lazy<IReadOnlyCollection<SeedRecipe>> Seed = new(LoadSeed);

        public Task<IReadOnlyCollection<Domain.Entities.Recipe>> GetRecipesAsync()
        {
            IReadOnlyCollection<Domain.Entities.Recipe> recipes = Seed.Value
                .Select(s => Domain.Entities.Recipe.Create(
                    externalId: s.ExternalId,
                    name: s.Name,
                    instructions: string.Join("\n", s.Steps),
                    ingredients: s.Ingredients.Select(i => new Domain.Entities.RecipeIngredient(i.Name, i.Measure)).ToList(),
                    imageUrl: s.ImageUrl ?? "",
                    tags: s.Tags))
                .ToList();
            return Task.FromResult(recipes);
        }

        private static IReadOnlyCollection<SeedRecipe> LoadSeed()
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(ResourceName)
                ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' not found.");
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<List<SeedRecipe>>(stream, options)
                ?? throw new InvalidOperationException($"Embedded resource '{ResourceName}' is empty.");
        }

        private sealed record SeedRecipe(
            string ExternalId,
            string Name,
            string? ImageUrl,
            string[] Tags,
            SeedIngredient[] Ingredients,
            string[] Steps);

        private sealed record SeedIngredient(string Name, string Measure);
    }
}
