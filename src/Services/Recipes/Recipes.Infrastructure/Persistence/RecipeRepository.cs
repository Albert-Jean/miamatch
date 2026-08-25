using System;
using System.Collections.Generic;
using System.Text;
using Recipes.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
namespace Recipes.Infrastructure.Persistence
{
    public class RecipeRepository : IRecipeRepository
    {
        private readonly RecipesDbContext _context;
        public RecipeRepository(RecipesDbContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> GetAllAsync()
        {
            var result = await _context.Recipes.ToListAsync();
            return result;
        }
        public async Task AddRangeAsync(IEnumerable<Recipes.Domain.Entities.Recipe> recipes)
        {
            await _context.Recipes.AddRangeAsync(recipes);
            await _context.SaveChangesAsync();
        }
        public async Task<IReadOnlyCollection<Recipes.Domain.Entities.Recipe>> GetByIdsAsync(IEnumerable<Guid> ids)
        {
            var result = await _context.Recipes.Where(r => ids.Contains(r.Id)).ToListAsync();
            return result;
        }
    }
}
