import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { CurrentDeck, RecipeDetails } from '../core/models/recipe.model';

@Injectable({ providedIn: 'root' })
export class RecipesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.recipesApiUrl;

  getCurrentDeck(householdId: string) {
    return this.http.get<CurrentDeck>(`${this.baseUrl}/decks/current`, { params: { householdId } });
  }

  generateDeck(householdId: string, categories: string[]) {
    return this.http.post<CurrentDeck>(`${this.baseUrl}/decks`, { householdId, categories });
  }

  getRecipe(id: string) {
    return this.http.get<RecipeDetails>(`${this.baseUrl}/recipes/${id}`);
  }
}
