export interface SwipeRequest {
  householdId: string;
  recipeId: string;
  deckId: string;
  liked: boolean;
}

export interface SwipeResponse {
  matched: boolean;
}

export interface Match {
  recipeId: string;
  name: string;
  imageUrl: string;
  matchedAt: string;
}
