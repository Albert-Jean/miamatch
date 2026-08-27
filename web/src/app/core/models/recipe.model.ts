export interface Recipe {
  id: string;
  name: string;
  imageUrl: string;
}

export interface CurrentDeck {
  deckId: string;
  recipes: Recipe[];
}
