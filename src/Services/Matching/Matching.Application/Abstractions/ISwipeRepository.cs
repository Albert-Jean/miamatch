using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Matching.Domain.Entities;

namespace Matching.Application.Abstractions
{
    public interface ISwipeRepository
    {
        Task<Swipe?> GetAsync(Guid userId,Guid householdId,Guid recipeId,Guid deckId);
        Task<IReadOnlyCollection<Swipe>?> GetForRecipeAsync(Guid householdId,Guid recipeId,Guid deckId);
        Task AddAsync(Swipe swipe);        
        Task UpdateAsync(Swipe swipe);
    }
}
