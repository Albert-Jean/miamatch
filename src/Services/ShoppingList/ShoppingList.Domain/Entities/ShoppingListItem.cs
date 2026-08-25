using System;
using System.Collections.Generic;
using System.Text;

namespace ShoppingList.Domain.Entities
{
    public class ShoppingListItem
    {
        public Guid Id { get; }
        public Guid HouseHoldId { get; }
        public Guid RecipeId { get; }
        public string IngredientName { get; }
        public string Measure { get; }
        public DateTime AddedAt { get; }

        private ShoppingListItem(Guid id, Guid houseHoldId, Guid recipeId, string ingredientName, string measure, DateTime addedAt)
        {
            Id = id;
            HouseHoldId = houseHoldId;
            RecipeId = recipeId;
            IngredientName = ingredientName;
            Measure = measure;
            AddedAt = addedAt;
        }
        public static ShoppingListItem Create(Guid householdId, Guid recipeId, string ingredientName, string measure)
        {
            Guid id = Guid.NewGuid();
            DateTime addedAt = DateTime.UtcNow;
            return new ShoppingListItem(id, householdId, recipeId, ingredientName, measure, addedAt);
        }
    }
}
