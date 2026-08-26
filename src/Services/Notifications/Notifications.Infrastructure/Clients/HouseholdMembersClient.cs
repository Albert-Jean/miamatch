using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using Notifications.Application.Abstractions;

namespace Notifications.Infrastructure.Clients
{
    public class HouseholdMembersClient: IHouseholdMembersClient
    {
        private readonly HttpClient _httpClient;
        public HouseholdMembersClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public async Task<IReadOnlyCollection<Guid>> GetMembersAsync(Guid householdId)
        {
            var response = await _httpClient.GetAsync($"households/{householdId}/members");
            if(response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return Array.Empty<Guid>();
            }
            else
            {
               return await response.Content.ReadFromJsonAsync<IReadOnlyCollection<Guid>>();
            }
        }
    }
}
