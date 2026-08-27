import { Component, OnInit, inject, signal } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { ToastService } from '../../core/toast/toast.service';
import { ShoppingListItem } from '../../core/models/shopping-list.model';
import { ShoppingListApiService } from '../../data-access/shopping-list-api.service';

@Component({
  selector: 'app-shopping-list',
  templateUrl: './shopping-list.component.html',
})
export class ShoppingListComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly shoppingListApi = inject(ShoppingListApiService);
  private readonly toast = inject(ToastService);

  protected readonly items = signal<ShoppingListItem[]>([]);
  protected readonly loading = signal(true);
  protected readonly checked = signal<ReadonlySet<string>>(new Set());
  protected readonly showText = signal(false);

  ngOnInit(): void {
    const householdId = this.auth.selectedHouseholdId();
    if (!householdId) return;

    this.shoppingListApi.getShoppingList(householdId).subscribe({
      next: (items) => {
        this.items.set(items);
        this.loading.set(false);
      },
      error: () => this.loading.set(false),
    });
  }

  formatMeasures(item: ShoppingListItem): string {
    return item.measures.map((m) => `${m.quantity} ${m.unit}`.trim()).join(' + ');
  }

  toggle(ingredientName: string): void {
    this.checked.update((current) => {
      const next = new Set(current);
      next.has(ingredientName) ? next.delete(ingredientName) : next.add(ingredientName);
      return next;
    });
  }

  buildText(): string {
    const lines = this.items().map((item) => `- ${item.ingredientName} — ${this.formatMeasures(item)}`);
    return `Liste de courses — ${this.items().length} ingrédient(s)\n\n${lines.join('\n')}`;
  }

  async copyList(): Promise<void> {
    try {
      await navigator.clipboard.writeText(this.buildText());
      this.toast.show('Liste copiée ✓');
    } catch {
      this.showText.set(true);
      this.toast.show('Copie manuelle ci-dessous');
    }
  }
}
