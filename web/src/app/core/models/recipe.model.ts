export interface Recipe {
  id: string;
  name: string;
  imageUrl: string;
  tags: string[];
}

export interface CurrentDeck {
  deckId: string;
  recipes: Recipe[];
}

export interface RecipeIngredient {
  name: string;
  measure: string;
}

export interface RecipeDetails {
  id: string;
  name: string;
  imageUrl: string;
  tags: string[];
  ingredients: RecipeIngredient[];
  steps: string[];
}

export interface RecipeCategory {
  slug: string;
  label: string;
  emoji: string;
}

/** Doit rester aligné avec les tags du catalogue seed backend (SeedData/recipes.json). */
export const RECIPE_CATEGORIES: RecipeCategory[] = [
  { slug: 'healthy', label: 'Healthy', emoji: '🥗' },
  { slug: 'proteine', label: 'Fort en protéines', emoji: '💪' },
  { slug: 'vegetarien', label: 'Végétarien', emoji: '🌱' },
  { slug: 'poisson', label: 'Poisson & mer', emoji: '🐟' },
  { slug: 'rapide', label: 'Rapide', emoji: '⏱️' },
  { slug: 'comfort', label: 'Réconfortant', emoji: '🍲' },
];

export function categoryLabel(slug: string): string {
  return RECIPE_CATEGORIES.find((c) => c.slug === slug)?.label ?? slug;
}

export function recipeEmoji(tags: string[]): string {
  const first = tags.find((t) => RECIPE_CATEGORIES.some((c) => c.slug === t));
  return RECIPE_CATEGORIES.find((c) => c.slug === first)?.emoji ?? '🍽️';
}
