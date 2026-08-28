export interface SwipeRequest {
  householdId: string;
  recipeId: string;
  deckId: string;
  liked: boolean;
}

export interface SwipeResponse {
  matched: boolean;
  matchCount: number;
  mealCount: number;
  weekComplete: boolean;
}

/** Où le foyer en est sur un deck : ce que l'utilisateur a déjà swipé, et si la semaine est bouclée. */
export interface DeckSwipeState {
  deckId: string;
  swipedRecipeIds: string[];
  matchCount: number;
  mealCount: number;
  weekComplete: boolean;
}

export interface Match {
  recipeId: string;
  name: string;
  imageUrl: string;
  matchedAt: string;
}
