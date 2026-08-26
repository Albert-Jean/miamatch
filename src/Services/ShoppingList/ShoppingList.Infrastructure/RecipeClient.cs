using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using ShoppingList.Application.Abstractions;

namespace ShoppingList.Infrastructure
{
    public class RecipeClient : IRecipeClient
    {
        private readonly HttpClient _httpClient;
        public RecipeClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<RecipeDetails?> GetRecipeAsync(Guid recipeId)
        {
            var response = await _httpClient.GetAsync($"recipes/{recipeId}");
            if (response.StatusCode== System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
            else
            {
                return await response.Content.ReadFromJsonAsync<RecipeDetails>();
            }
        }
    }
}
