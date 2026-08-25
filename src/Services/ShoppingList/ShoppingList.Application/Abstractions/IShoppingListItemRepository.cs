using System;
using System.Collections.Generic;
using System.Text;
using ShoppingList.Domain.Entities;

namespace ShoppingList.Application.Abstractions
{
    public interface IShoppingListItemRepository
    {
        Task<IReadOnlyCollection<ShoppingListItem>> GetForHouseholdAsync(Guid householdId);
        Task AddAsync(ShoppingListItem item);
    }
}
