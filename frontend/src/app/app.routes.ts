import { Routes } from '@angular/router';
import {
  authGuard,
  noAuthGuard,
  onboardingGuard,
  needsOnboardingGuard,
  creatorGuard,
  adminGuard,
  retailerGuard,
} from './core';

export const routes: Routes = [
  // Public routes
  {
    path: '',
    redirectTo: 'feed',
    pathMatch: 'full',
  },

  // Auth routes (only accessible when NOT logged in)
  {
    path: 'auth',
    canActivate: [noAuthGuard],
    children: [
      {
        path: '',
        redirectTo: 'login',
        pathMatch: 'full',
      },
      {
        path: 'login',
        loadComponent: () =>
          import('./features/auth/login/login.page').then((m) => m.LoginPage),
      },
      {
        path: 'register',
        loadComponent: () =>
          import('./features/auth/register/register.page').then(
            (m) => m.RegisterPage
          ),
      },
      {
        path: 'forgot-password',
        loadComponent: () =>
          import('./features/auth/forgot-password/forgot-password.page').then(
            (m) => m.ForgotPasswordPage
          ),
      },
      {
        path: 'reset-password',
        loadComponent: () =>
          import('./features/auth/reset-password/reset-password.page').then(
            (m) => m.ResetPasswordPage
          ),
      },
    ],
  },

  // Onboarding (requires auth, but NOT completed body profile)
  {
    path: 'onboarding',
    canActivate: [authGuard, needsOnboardingGuard],
    loadComponent: () =>
      import('./features/onboarding/onboarding.page').then(
        (m) => m.OnboardingPage
      ),
  },

  // Main app routes (requires auth AND completed onboarding)
  {
    path: 'feed',
    canActivate: [authGuard, onboardingGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/feed/feed-home/feed-home.page').then(
            (m) => m.FeedHomePage
          ),
      },
      {
        path: 'explore',
        loadComponent: () =>
          import('./features/feed/explore/explore.page').then(
            (m) => m.ExplorePage
          ),
      },
      {
        path: 'saved',
        loadComponent: () =>
          import('./features/feed/saved/saved.page').then((m) => m.SavedPage),
      },
      {
        path: 'post/:id',
        loadComponent: () =>
          import('./features/feed/post-detail/post-detail.page').then(
            (m) => m.PostDetailPage
          ),
      },
    ],
  },

  // Profile
  {
    path: 'profile',
    canActivate: [authGuard, onboardingGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/profile/profile-view/profile-view.page').then(
            (m) => m.ProfileViewPage
          ),
      },
      {
        path: 'edit',
        loadComponent: () =>
          import('./features/profile/profile-edit/profile-edit.page').then(
            (m) => m.ProfileEditPage
          ),
      },
      {
        path: 'body-profile',
        loadComponent: () =>
          import(
            './features/profile/body-profile-edit/body-profile-edit.page'
          ).then((m) => m.BodyProfileEditPage),
      },
    ],
  },

  // Settings
  {
    path: 'settings',
    canActivate: [authGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/settings/settings/settings.page').then(
            (m) => m.SettingsPage
          ),
      },
      {
        path: 'privacy',
        loadComponent: () =>
          import('./features/settings/privacy/privacy.page').then(
            (m) => m.PrivacyPage
          ),
      },
      {
        path: 'delete-account',
        loadComponent: () =>
          import('./features/settings/delete-account/delete-account.page').then(
            (m) => m.DeleteAccountPage
          ),
      },
    ],
  },

  // Commerce
  {
    path: 'product/:id',
    canActivate: [authGuard, onboardingGuard],
    loadComponent: () =>
      import('./features/commerce/product-detail/product-detail.page').then(
        (m) => m.ProductDetailPage
      ),
  },
  {
    path: 'shop',
    canActivate: [authGuard, onboardingGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            './features/commerce/product-search/product-search.page'
          ).then((m) => m.ProductSearchPage),
      },
      {
        path: 'retailers',
        loadComponent: () =>
          import(
            './features/commerce/retailer-list/retailer-list.page'
          ).then((m) => m.RetailerListPage),
      },
      {
        path: 'retailers/:id',
        loadComponent: () =>
          import(
            './features/commerce/retailer-storefront/retailer-storefront.page'
          ).then((m) => m.RetailerStorefrontPage),
      },
    ],
  },

  // Creator registration (accessible to any authenticated user)
  {
    path: 'creator/register',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/creator/register/register.page').then(
        (m) => m.RegisterPage
      ),
  },

  // Creator routes (requires creator role)
  {
    path: 'creator',
    canActivate: [authGuard, creatorGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/creator/dashboard/dashboard.page').then(
            (m) => m.DashboardPage
          ),
      },
      {
        path: 'posts',
        loadComponent: () =>
          import('./features/creator/posts/posts.page').then(
            (m) => m.PostsPage
          ),
      },
      {
        path: 'posts/create',
        loadComponent: () =>
          import('./features/creator/post-create/post-create.page').then(
            (m) => m.PostCreatePage
          ),
      },
      {
        path: 'posts/:id/edit',
        loadComponent: () =>
          import('./features/creator/post-edit/post-edit.page').then(
            (m) => m.PostEditPage
          ),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import('./features/creator/analytics/analytics.page').then(
            (m) => m.AnalyticsPage
          ),
      },
      {
        path: 'earnings',
        loadComponent: () =>
          import('./features/creator/earnings/earnings.page').then(
            (m) => m.EarningsPage
          ),
      },
      {
        path: 'verification',
        loadComponent: () =>
          import('./features/creator/verification/verification.page').then(
            (m) => m.VerificationPage
          ),
      },
    ],
  },

  // Retailer registration (accessible to any authenticated user)
  {
    path: 'retailer/register',
    canActivate: [authGuard],
    loadComponent: () =>
      import('./features/retailer/register/register.page').then(
        (m) => m.RetailerRegisterPage
      ),
  },

  // Retailer routes (requires retailer role)
  {
    path: 'retailer',
    canActivate: [authGuard, retailerGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import(
            './features/retailer/retailer-dashboard/retailer-dashboard.page'
          ).then((m) => m.RetailerDashboardPage),
      },
      {
        path: 'profile',
        loadComponent: () =>
          import(
            './features/retailer/retailer-profile/retailer-profile.page'
          ).then((m) => m.RetailerProfilePage),
      },
      {
        path: 'products',
        loadComponent: () =>
          import('./features/retailer/products/products.page').then(
            (m) => m.ProductsPage
          ),
      },
      {
        path: 'products/:id/edit',
        loadComponent: () =>
          import('./features/retailer/product-edit/product-edit.page').then(
            (m) => m.ProductEditPage
          ),
      },
      {
        path: 'campaigns',
        loadComponent: () =>
          import('./features/retailer/campaigns/campaigns.page').then(
            (m) => m.CampaignsPage
          ),
      },
      {
        path: 'campaigns/create',
        loadComponent: () =>
          import(
            './features/retailer/campaign-create/campaign-create.page'
          ).then((m) => m.CampaignCreatePage),
      },
      {
        path: 'campaigns/:id/edit',
        loadComponent: () =>
          import(
            './features/retailer/campaign-create/campaign-create.page'
          ).then((m) => m.CampaignCreatePage),
      },
      {
        path: 'analytics',
        loadComponent: () =>
          import(
            './features/retailer/retailer-analytics/retailer-analytics.page'
          ).then((m) => m.RetailerAnalyticsPage),
      },
      {
        path: 'insights',
        loadComponent: () =>
          import('./features/retailer/insights/insights.page').then(
            (m) => m.InsightsPage
          ),
      },
    ],
  },

  // Admin routes (requires admin role)
  {
    path: 'admin',
    canActivate: [authGuard, adminGuard],
    children: [
      {
        path: '',
        loadComponent: () =>
          import('./features/admin/admin-dashboard/admin-dashboard.page').then(
            (m) => m.AdminDashboardPage
          ),
      },
      {
        path: 'posts',
        loadComponent: () =>
          import('./features/admin/post-moderation/post-moderation.page').then(
            (m) => m.PostModerationPage
          ),
      },
      {
        path: 'users',
        loadComponent: () =>
          import('./features/admin/user-management/user-management.page').then(
            (m) => m.UserManagementPage
          ),
      },
      {
        path: 'users/:id',
        loadComponent: () =>
          import('./features/admin/user-detail/user-detail.page').then(
            (m) => m.UserDetailPage
          ),
      },
      {
        path: 'verifications',
        loadComponent: () =>
          import(
            './features/admin/verification-review/verification-review.page'
          ).then((m) => m.VerificationReviewPage),
      },
    ],
  },

  // Catch-all redirect
  {
    path: '**',
    redirectTo: 'feed',
  },
];
