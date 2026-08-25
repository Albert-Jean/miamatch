using System;
using System.Collections.Generic;
using System.Text;
using Recipes.Application.Abstractions;
using Microsoft.EntityFrameworkCore;
using Recipe.Domain.Entities;
namespace Recipes.Infrastructure.Persistence
{
    public class DeckRepository : IDeckRepository
    {
        private readonly RecipesDbContext _context;
        public DeckRepository (RecipesDbContext context)
        {
            _context = context;
        }       
        public async Task AddAsync(Deck deck)
        {
            await _context.Decks.AddAsync(deck);
            await _context.SaveChangesAsync();
        }
        public async Task<Deck?> GetMostRecentAsync(Guid householdId)
        {
            var result = await _context.Decks
                .Where(d => d.HouseholdId == householdId)
                .OrderByDescending(d => d.GeneratedAt)
                .FirstOrDefaultAsync();
            return result;
        }
    }
}
