using System;
using System.Collections.Generic;
using System.Text;
using Matching.Application.Abstractions;
using Matching.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Matching.Infrastructure.Persistence.Repositories
{
    public class MatchRepository: IMatchRepository
    {
        private readonly MatchingDbContext _context;

        public MatchRepository(MatchingDbContext context)
        {
            _context = context;
        }

        public async Task<Match?> GetAsync(Guid householdId, Guid recipeId, Guid deckId)
        {
            return await _context.Matches
                .FirstOrDefaultAsync(m => m.HouseholdId == householdId && m.RecipeId == recipeId && m.DeckId == deckId);
        }
        public async Task<IReadOnlyCollection<Match>> GetForHouseholdAsync(Guid householdId)
        {
            return await _context.Matches
                .Where(m => m.HouseholdId == householdId).ToListAsync();
        }

        public async Task<int> CountForDeckAsync(Guid householdId, Guid deckId)
        {
            return await _context.Matches
                .CountAsync(m => m.HouseholdId == householdId && m.DeckId == deckId);
        }

        public async Task AddAsync(Match match)
        {
            await _context.Matches.AddAsync(match);
            await _context.SaveChangesAsync();
        }        
    }
}
