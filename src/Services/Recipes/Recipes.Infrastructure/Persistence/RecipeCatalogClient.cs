using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Recipes.Application.Abstractions;

namespace Recipes.Infrastructure.Persistence
{
    public class RecipeCatalogClient: IRecipeCatalogClient
    {
        private readonly HttpClient _httpClient;
        private List<string> Categories = new List<string> { "Beef", "Breakfast", "Chicken", "Dessert","Goat", "Lamb","Miscellaneous", "Pasta", "Pork", "Seafood", "Side", "Starter", "Vegan", "Vegetarian" };
        public RecipeCatalogClient(HttpClient httpClient) 
        {
            _httpClient = httpClient;
        }
        public async Task<IReadOnlyCollection<Domain.Entities.Recipe>> FetchRecipesAsync(int minimumCount)
        {
            string category = Categories[Random.Shared.Next(Categories.Count)];
            var recipesFromCategory = await _httpClient.GetFromJsonAsync<JsonElement>($"filter.php?c={category}");
            var meals = recipesFromCategory.GetProperty("meals");
            List<Domain.Entities.Recipe> recipes = new List<Domain.Entities.Recipe>();
            List<string> recipesIds = meals.EnumerateArray()
                .Select(m => m.GetProperty("idMeal").GetString()!)
                .OrderBy(_ => Random.Shared.Next())
                .Take(minimumCount)
                .ToList();

            foreach (var recipe in recipesIds)
            {
                var recipeDetails = await _httpClient.GetFromJsonAsync<JsonElement>($"lookup.php?i={recipe}");
                var meal = recipeDetails.GetProperty("meals")[0];
                var ingredients = new List<Domain.Entities.RecipeIngredient>();
                for (int i = 1; i <= 20; i++)
                {
                    var ingredientName = meal.GetProperty($"strIngredient{i}").GetString();
                    var ingredientMeasure = meal.GetProperty($"strMeasure{i}").GetString();
                    if (!string.IsNullOrWhiteSpace(ingredientName))
                    {
                        ingredients.Add(new Domain.Entities.RecipeIngredient(ingredientName, ingredientMeasure ?? ""));
                    }
                }
                 recipes.Add(Domain.Entities.Recipe.Create(
                    mealDbId: meal.GetProperty("idMeal").GetString()!,
                    name: meal.GetProperty("strMeal").GetString()!,
                    instructions: meal.GetProperty("strInstructions").GetString()!,
                    ingredients: ingredients,
                    imageUrl: meal.GetProperty("strMealThumb").GetString()!
                ));
            }
            return recipes;
        }

    }
}
