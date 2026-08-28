using System;
using System.Collections.Generic;
using System.Text;
using Matching.Application.Abstractions;
using Matching.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace Matching.Infrastructure.Persistence.Repositories
{
    public class SwipeRepository: ISwipeRepository
    {
        private readonly MatchingDbContext _context;

        public SwipeRepository(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<Swipe?> GetAsync(Guid userId, Guid householdId, Guid recipeId, Guid deckId)
        {
            return await _context.Swipes
                .FirstOrDefaultAsync(s => s.UserId == userId && s.HouseholdId == householdId && s.RecipeId == recipeId && s.DeckId == deckId);
        }
        public async Task<IReadOnlyCollection<Swipe>> GetForRecipeAsync(Guid householdId, Guid recipeId, Guid deckId)
        {
            return await _context.Swipes
                .Where(s => s.HouseholdId == householdId && s.RecipeId == recipeId && s.DeckId == deckId)
                .ToListAsync();
        }

        public async Task<IReadOnlyCollection<Swipe>> GetForUserAndDeckAsync(Guid userId, Guid householdId, Guid deckId)
        {
            return await _context.Swipes
                .Where(s => s.UserId == userId && s.HouseholdId == householdId && s.DeckId == deckId)
                .ToListAsync();
        }

        public async Task AddAsync(Swipe swipe)
        {
            await _context.Swipes.AddAsync(swipe);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Swipe swipe)
        {
            _context.Swipes.Update(swipe);
            await _context.SaveChangesAsync();
        }
    }
}
