using System;
using System.Collections.Generic;
using System.Text;
using Recipe.Domain.Entities;

namespace Recipes.Application.Abstractions
{
    public interface IDeckRepository
    {
        public Task AddAsync(Deck deck);
        public Task<Deck?> GetMostRecentAsync(Guid householdId);
    }
}
