import { Component, OnInit, computed, inject, signal } from '@angular/core';
import { AuthService } from '../../core/auth/auth.service';
import { MatchesStoreService } from '../../core/matches/matches-store.service';
import { ToastService } from '../../core/toast/toast.service';
import { CurrentDeck, RECIPE_CATEGORIES, Recipe, categoryLabel, recipeEmoji } from '../../core/models/recipe.model';
import { MatchingApiService } from '../../data-access/matching-api.service';
import { RecipesApiService } from '../../data-access/recipes-api.service';

const SWIPE_THRESHOLD_PX = 110;

type SwipeDirection = 'like' | 'pass';

interface DragState {
  dx: number;
  dy: number;
  active: boolean;
}

@Component({
  selector: 'app-swipe-deck',
  templateUrl: './swipe-deck.component.html',
})
export class SwipeDeckComponent implements OnInit {
  private readonly auth = inject(AuthService);
  private readonly recipesApi = inject(RecipesApiService);
  private readonly matchingApi = inject(MatchingApiService);
  private readonly matchesStore = inject(MatchesStoreService);
  private readonly toast = inject(ToastService);

  private deck: CurrentDeck | null = null;
  private pointerStart: { x: number; y: number } | null = null;

  protected readonly categories = RECIPE_CATEGORIES;
  protected readonly recipeEmoji = recipeEmoji;
  protected readonly categoryLabel = categoryLabel;

  protected readonly recipes = signal<Recipe[]>([]);
  protected readonly index = signal(0);
  protected readonly history = signal<string[]>([]);
  protected readonly loading = signal(true);
  protected readonly needsDeck = signal(false);
  protected readonly generating = signal(false);
  protected readonly selectedCategories = signal<string[]>([]);
  protected readonly matchModal = signal<Recipe | null>(null);
  protected readonly drag = signal<DragState>({ dx: 0, dy: 0, active: false });
  protected readonly exit = signal<SwipeDirection | null>(null);

  protected readonly visibleStack = computed(() => this.recipes().slice(this.index(), this.index() + 3).reverse());
  protected readonly done = computed(() => !this.loading() && !this.needsDeck() && this.index() >= this.recipes().length);
  protected readonly confetti = Array.from({ length: 14 }, (_, i) => i);

  protected readonly likeOpacity = computed(() => {
    if (this.exit() === 'like') return 1;
    return Math.max(0, Math.min(1, this.drag().dx / SWIPE_THRESHOLD_PX));
  });
  protected readonly passOpacity = computed(() => {
    if (this.exit() === 'pass') return 1;
    return Math.max(0, Math.min(1, -this.drag().dx / SWIPE_THRESHOLD_PX));
  });

  ngOnInit(): void {
    const householdId = this.auth.selectedHouseholdId();
    if (!householdId) return;

    this.recipesApi.getCurrentDeck(householdId).subscribe({
      next: (deck) => this.setDeck(deck),
      error: (err) => {
        this.loading.set(false);
        this.needsDeck.set(true);
        if (err?.status !== 404) {
          this.toast.show('Deck indisponible (API injoignable ?)');
        }
      },
    });
  }

  isSelected(slug: string): boolean {
    return this.selectedCategories().includes(slug);
  }

  toggleCategory(slug: string): void {
    this.selectedCategories.update((selected) =>
      selected.includes(slug) ? selected.filter((s) => s !== slug) : [...selected, slug],
    );
  }

  generateDeck(): void {
    const householdId = this.auth.selectedHouseholdId();
    if (!householdId || this.generating()) return;

    this.generating.set(true);
    this.recipesApi.generateDeck(householdId, this.selectedCategories()).subscribe({
      next: (deck) => {
        this.generating.set(false);
        this.setDeck(deck);
      },
      error: () => {
        this.generating.set(false);
        this.toast.show('Impossible de générer le deck');
      },
    });
  }

  private setDeck(deck: CurrentDeck): void {
    this.deck = deck;
    this.recipes.set(deck.recipes);
    this.index.set(0);
    this.history.set([]);
    this.needsDeck.set(false);
    this.loading.set(false);
  }

  topTransform(isTop: boolean, depth: number): string {
    if (!isTop) {
      return `translateY(${depth * 10}px) scale(${1 - depth * 0.04})`;
    }
    if (this.exit()) {
      const offset = this.exit() === 'like' ? 600 : -600;
      const rotation = this.exit() === 'like' ? 24 : -24;
      return `translate(${offset}px, 40px) rotate(${rotation}deg)`;
    }
    const { dx, dy } = this.drag();
    return `translate(${dx}px, ${dy}px) rotate(${dx * 0.05}deg)`;
  }

  onPointerDown(event: PointerEvent): void {
    if (this.exit()) return;
    this.pointerStart = { x: event.clientX, y: event.clientY };
    this.drag.update((d) => ({ ...d, active: true }));
    (event.currentTarget as HTMLElement).setPointerCapture?.(event.pointerId);
  }

  onPointerMove(event: PointerEvent): void {
    if (!this.pointerStart) return;
    this.drag.set({
      dx: event.clientX - this.pointerStart.x,
      dy: event.clientY - this.pointerStart.y,
      active: true,
    });
  }

  onPointerUp(): void {
    if (!this.pointerStart) return;
    const { dx } = this.drag();
    this.pointerStart = null;
    if (dx > SWIPE_THRESHOLD_PX) this.commit('like');
    else if (dx < -SWIPE_THRESHOLD_PX) this.commit('pass');
    else this.drag.set({ dx: 0, dy: 0, active: false });
  }

  commit(direction: SwipeDirection): void {
    const recipe = this.recipes()[this.index()];
    if (!recipe || this.exit()) return;

    const liked = direction === 'like';
    this.exit.set(direction);
    setTimeout(() => {
      this.history.update((h) => [...h, recipe.id]);
      this.index.update((i) => i + 1);
      this.drag.set({ dx: 0, dy: 0, active: false });
      this.exit.set(null);
    }, 280);

    const householdId = this.auth.selectedHouseholdId();
    const deckId = this.deck?.deckId;
    if (!householdId || !deckId) return;

    this.matchingApi.swipe({ householdId, recipeId: recipe.id, deckId, liked }).subscribe({
      next: (response) => {
        if (liked && response.matched) {
          this.matchModal.set(recipe);
          this.matchesStore.refresh(householdId);
        }
      },
      error: () => this.toast.show('Swipe non enregistré (API injoignable ?)'),
    });
  }

  undo(): void {
    if (!this.history().length || this.exit()) return;
    this.history.update((h) => h.slice(0, -1));
    this.index.update((i) => Math.max(0, i - 1));
  }
}
