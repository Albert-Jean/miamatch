export interface IngredientMeasure {
  quantity: number;
  unit: string;
  recipeName: string;
}

export interface ShoppingListItem {
  ingredientName: string;
  measures: IngredientMeasure[];
}
