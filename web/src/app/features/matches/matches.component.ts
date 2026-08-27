import { Component, inject, signal } from '@angular/core';
import { MatchesStoreService } from '../../core/matches/matches-store.service';
import { ToastService } from '../../core/toast/toast.service';
import { Match } from '../../core/models/matching.model';
import { RecipeDetails, categoryLabel, recipeEmoji } from '../../core/models/recipe.model';
import { RecipesApiService } from '../../data-access/recipes-api.service';

@Component({
  selector: 'app-matches',
  templateUrl: './matches.component.html',
})
export class MatchesComponent {
  protected readonly matchesStore = inject(MatchesStoreService);
  private readonly recipesApi = inject(RecipesApiService);
  private readonly toast = inject(ToastService);

  protected readonly categoryLabel = categoryLabel;
  protected readonly recipeEmoji = recipeEmoji;

  protected readonly detail = signal<RecipeDetails | null>(null);
  protected readonly detailLoading = signal(false);

  openRecipe(match: Match): void {
    if (this.detailLoading()) return;
    this.detailLoading.set(true);
    this.recipesApi.getRecipe(match.recipeId).subscribe({
      next: (details) => {
        this.detailLoading.set(false);
        this.detail.set(details);
      },
      error: () => {
        this.detailLoading.set(false);
        this.toast.show('Recette introuvable');
      },
    });
  }

  closeRecipe(): void {
    this.detail.set(null);
  }
}
