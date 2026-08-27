import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';
import { householdGuard } from './core/guards/household.guard';
import { ShellComponent } from './shell/shell.component';

export const routes: Routes = [
  {
    path: 'onboarding',
    loadComponent: () =>
      import('./features/onboarding/onboarding.component').then((m) => m.OnboardingComponent),
  },
  {
    path: '',
    component: ShellComponent,
    canActivate: [authGuard, householdGuard],
    children: [
      { path: '', redirectTo: 'swipe', pathMatch: 'full' },
      {
        path: 'swipe',
        loadComponent: () =>
          import('./features/swipe-deck/swipe-deck.component').then((m) => m.SwipeDeckComponent),
      },
      {
        path: 'matches',
        loadComponent: () => import('./features/matches/matches.component').then((m) => m.MatchesComponent),
      },
      {
        path: 'shopping-list',
        loadComponent: () =>
          import('./features/shopping-list/shopping-list.component').then((m) => m.ShoppingListComponent),
      },
    ],
  },
  { path: '**', redirectTo: 'swipe' },
];
