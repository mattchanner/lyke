import { Routes } from '@angular/router';
import { authGuard } from '../../core/guards';

export const STYLE_QUIZ_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () =>
      import('./pages/landing/style-quiz-landing.page').then(
        (m) => m.StyleQuizLandingPage
      ),
  },
  {
    path: 'primer',
    loadComponent: () =>
      import('./pages/primer/style-quiz-primer.page').then(
        (m) => m.StyleQuizPrimerPage
      ),
  },
  {
    path: 'section/:sectionId',
    loadComponent: () =>
      import('./pages/section-intro/style-quiz-section-intro.page').then(
        (m) => m.StyleQuizSectionIntroPage
      ),
  },
  {
    path: 'question/:questionId',
    loadComponent: () =>
      import('./pages/question/style-quiz-question.page').then(
        (m) => m.StyleQuizQuestionPage
      ),
  },
  {
    path: 'calculating',
    loadComponent: () =>
      import('./pages/calculating/style-quiz-calculating.page').then(
        (m) => m.StyleQuizCalculatingPage
      ),
  },
  {
    path: 'teaser',
    loadComponent: () =>
      import('./pages/teaser/style-quiz-teaser.page').then(
        (m) => m.StyleQuizTeaserPage
      ),
  },
  {
    // T3.8: auth guard on full results route
    path: 'results',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./pages/results/style-quiz-results.page').then(
        (m) => m.StyleQuizResultsPage
      ),
  },
];
