using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using ShoppingList.Application.Abstractions;
using ShoppingList.Domain.Entities;
using ShoppingList.Infrastructure.Persistence;

namespace ShoppingList.Infrastructure.Repositories
{
    public class ShoppingListItemRepository: IShoppingListItemRepository
    {
        private readonly ShoppingListDbContext _context;
        public ShoppingListItemRepository(ShoppingListDbContext context)
        {
            _context = context;
        }
        public async Task<IReadOnlyCollection<ShoppingListItem>> GetForHouseholdAsync(Guid householdId)
        {
            return await _context.ShoppingListItems.Where(i => i.HouseholdId == householdId).ToListAsync();
        }
        public async Task AddAsync(ShoppingListItem item)
        {
            await _context.ShoppingListItems.AddAsync(item);
            await _context.SaveChangesAsync();
        }
    }
}
