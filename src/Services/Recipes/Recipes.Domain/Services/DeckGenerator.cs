using System;
using System.Collections.Generic;
using System.Text;

namespace Recipes.Domain.Services
{
    public static class DeckGenerator
    {     
        public static IReadOnlyCollection<Guid> GenerateRecipeIds(IReadOnlyCollection<Guid> availableRecipeIds, IReadOnlyCollection<Guid> excludedRecipeIds, int count)
        {
            var result = availableRecipeIds.Except(excludedRecipeIds);
            if(result.Count() >= count)
            {
                return result.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
            }
            else
            {
                return availableRecipeIds.OrderBy(_ => Random.Shared.Next()).Take(count).ToList();
            }
        }
    }
}
