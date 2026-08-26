using System;
using System.Collections.Generic;
using System.Text;
using Recipes.Domain.Entities;

namespace Recipes.Application.Abstractions
{
    public interface IRecipeRepository
    {
        public Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> GetAllAsync();
        public Task AddRangeAsync(IEnumerable<Recipes.Domain.Entities.Recipe> recipes);
        public Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> GetByIdsAsync(IEnumerable<Guid> ids);
        public Task<Recipes.Domain.Entities.Recipe?> GetByIdAsync(Guid id);
    }
}
