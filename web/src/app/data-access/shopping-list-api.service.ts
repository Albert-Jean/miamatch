import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { environment } from '../../environments/environment';
import { ShoppingListItem } from '../core/models/shopping-list.model';

@Injectable({ providedIn: 'root' })
export class ShoppingListApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.shoppingListApiUrl;

  getShoppingList(householdId: string) {
    return this.http.get<ShoppingListItem[]>(`${this.baseUrl}/shopping-list`, { params: { householdId } });
  }
}
