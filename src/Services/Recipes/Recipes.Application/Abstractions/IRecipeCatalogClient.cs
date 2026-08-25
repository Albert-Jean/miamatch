using System;
using System.Collections.Generic;
using System.Text;

namespace Recipes.Application.Abstractions
{
    public interface IRecipeCatalogClient
    {
        public Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> FetchRecipesAsync(int minimumCount);
    }
}
