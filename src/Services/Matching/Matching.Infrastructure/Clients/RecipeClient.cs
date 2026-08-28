using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Matching.Application.Abstractions;

namespace Matching.Infrastructure.Clients
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
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<RecipeDetails>();
        }

        public async Task<DeckSummary?> GetDeckAsync(Guid deckId)
        {
            var response = await _httpClient.GetAsync($"decks/{deckId}");
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            return await response.Content.ReadFromJsonAsync<DeckSummary>();
        }
    }
}
