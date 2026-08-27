import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { CurrentDeck } from '../core/models/recipe.model';

@Injectable({ providedIn: 'root' })
export class RecipesApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.recipesApiUrl;

  getCurrentDeck(householdId: string) {
    return this.http.get<CurrentDeck>(`${this.baseUrl}/decks/current`, { params: { householdId } });
  }
}
