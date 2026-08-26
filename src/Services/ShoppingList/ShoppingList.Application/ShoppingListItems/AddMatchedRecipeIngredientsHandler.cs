using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using ShoppingList.Application.Abstractions;
using ShoppingList.Domain.Entities;

namespace ShoppingList.Application.ShoppingListItems
{
    public class AddMatchedRecipeIngredientsHandler
    {
        private readonly IRecipeClient _recipeClient;
        private readonly IShoppingListItemRepository _shoppingListItemRepository;
        public AddMatchedRecipeIngredientsHandler(IRecipeClient recipeClient, IShoppingListItemRepository shoppingListItemRepository)
        {
            _recipeClient = recipeClient;
            _shoppingListItemRepository = shoppingListItemRepository;
        }

        public async Task ExecuteAsync(Guid householdId, Guid recipeId)
        {
            var recipe =  await _recipeClient.GetRecipeAsync(recipeId);
            if(recipe is null)
            {
                return ;
            }
            foreach(var ingredient in recipe.Ingredients)
            {
                var shoppingListItem = ShoppingListItem.Create(householdId, recipeId, ingredient.Name, ingredient.Measure);
                await _shoppingListItemRepository.AddAsync(shoppingListItem);
            }
        }
    }
}
