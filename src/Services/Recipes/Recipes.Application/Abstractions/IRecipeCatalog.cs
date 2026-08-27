using System;
using System.Collections.Generic;
using System.Text;

namespace Recipes.Application.Abstractions
{
    public interface IRecipeCatalog
    {
        public Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> GetRecipesAsync();
    }
}
