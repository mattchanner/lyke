import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonIcon,
  IonButtons,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonSpinner,
  IonChip,
  IonLabel,
  InfiniteScrollCustomEvent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  searchOutline,
  bookmarkOutline,
  personCircleOutline,
  filterOutline,
} from 'ionicons/icons';
import { ApiService, AuthService } from '../../../core';
import { FeedPostResponse, FeedSortBy } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';
import { SkeletonPostCardComponent } from '../../../shared/components/loading-skeleton';

@Component({
  selector: 'app-feed-home',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonIcon,
    IonButtons,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonSpinner,
    IonChip,
    IonLabel,
    PostCardComponent,
    SkeletonPostCardComponent,
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-title>LYKE</ion-title>
        <ion-buttons slot="end">
          <ion-button routerLink="/feed/explore">
            <ion-icon name="search-outline" slot="icon-only"></ion-icon>
          </ion-button>
          <ion-button routerLink="/feed/saved">
            <ion-icon name="bookmark-outline" slot="icon-only"></ion-icon>
          </ion-button>
          <ion-button routerLink="/profile">
            <ion-icon name="person-circle-outline" slot="icon-only"></ion-icon>
          </ion-button>
        </ion-buttons>
      </ion-toolbar>

      <ion-toolbar>
        <div class="sort-chips">
          <ion-chip
            [color]="sortBy() === FeedSortBy.Relevance ? 'primary' : 'medium'"
            (click)="setSortBy(FeedSortBy.Relevance)"
          >
            <ion-label>For You</ion-label>
          </ion-chip>
          <ion-chip
            [color]="sortBy() === FeedSortBy.Recent ? 'primary' : 'medium'"
            (click)="setSortBy(FeedSortBy.Recent)"
          >
            <ion-label>Recent</ion-label>
          </ion-chip>
          <ion-chip
            [color]="sortBy() === FeedSortBy.MostLiked ? 'primary' : 'medium'"
            (click)="setSortBy(FeedSortBy.MostLiked)"
          >
            <ion-label>Popular</ion-label>
          </ion-chip>
        </div>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <ion-refresher slot="fixed" (ionRefresh)="onRefresh($event)">
        <ion-refresher-content></ion-refresher-content>
      </ion-refresher>

      @if (isLoading() && posts().length === 0) {
        <div class="skeleton-feed">
          @for (i of [1, 2, 3]; track i) {
            <app-skeleton-post-card></app-skeleton-post-card>
          }
        </div>
      } @else if (!isLoading() && posts().length === 0) {
        <div class="empty-state">
          <h3>No posts yet</h3>
          <!-- <p>Complete your body profile to see personalized outfit recommendations!</p>
          <ion-button routerLink="/profile/body-profile">
            Update Body Profile
          </ion-button> -->
        </div>
      } @else {
        <div class="feed-grid">
          @for (post of posts(); track post.id) {
            <app-post-card
              [post]="post"
              (liked)="onPostLiked($event)"
              (saved)="onPostSaved($event)"
            ></app-post-card>
          }
        </div>

        <ion-infinite-scroll
          (ionInfinite)="loadMore($event)"
          [disabled]="!hasMore()"
        >
          <ion-infinite-scroll-content
            loadingSpinner="crescent"
            loadingText="Loading more..."
          ></ion-infinite-scroll-content>
        </ion-infinite-scroll>
      }
    </ion-content>
  `,
  styles: [`
    .sort-chips {
      display: flex;
      gap: 0.5rem;
      padding: 0 1rem;
      overflow-x: auto;

      ion-chip {
        flex-shrink: 0;
      }
    }

    .skeleton-feed {
      display: flex;
      flex-direction: column;
      gap: 1px;
      background: var(--ion-color-light);
    }

    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      height: 50vh;
      padding: 2rem;
      text-align: center;

      h3 {
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--ion-color-medium);
        margin-bottom: 1.5rem;
      }
    }

    .feed-grid {
      display: grid;
      grid-template-columns: 1fr;
      gap: 1px;
      background: var(--ion-color-light);

      @media (min-width: 768px) {
        grid-template-columns: repeat(2, 1fr);
      }

      @media (min-width: 1024px) {
        grid-template-columns: repeat(3, 1fr);
      }
    }
  `],
})
export class FeedHomePage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);

  readonly FeedSortBy = FeedSortBy;

  readonly posts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);
  readonly sortBy = signal(FeedSortBy.Relevance);
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);

  constructor() {
    addIcons({
      searchOutline,
      bookmarkOutline,
      personCircleOutline,
      filterOutline,
    });
  }

  ngOnInit(): void {
    this.loadFeed();
  }

  loadFeed(refresh = false): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }

    this.isLoading.set(true);

    this.api.get<FeedPostResponse[]>('feed', '', {
      page: this.currentPage(),
      pageSize: 20,
      sortBy: this.sortBy(),
    }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          if (refresh) {
            this.posts.set(response.data);
          } else {
            this.posts.update((current) => [...current, ...response.data!]);
          }

          if (response.meta) {
            this.hasMore.set(response.meta.hasNextPage);
          }
        }
      },
      error: () => {
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  setSortBy(sort: FeedSortBy): void {
    if (this.sortBy() !== sort) {
      this.sortBy.set(sort);
      this.loadFeed(true);
    }
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadFeed(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadFeed();
    setTimeout(() => event.target.complete(), 1000);
  }

  onPostLiked(event: { postId: string; liked: boolean }): void {
    this.posts.update((posts) =>
      posts.map((p) =>
        p.id === event.postId
          ? {
              ...p,
              isLiked: event.liked,
              engagements: {
                ...p.engagements,
                likes: p.engagements.likes + (event.liked ? 1 : -1),
              },
            }
          : p
      )
    );
  }

  onPostSaved(event: { postId: string; saved: boolean }): void {
    this.posts.update((posts) =>
      posts.map((p) =>
        p.id === event.postId
          ? {
              ...p,
              isSaved: event.saved,
              engagements: {
                ...p.engagements,
                saves: p.engagements.saves + (event.saved ? 1 : -1),
              },
            }
          : p
      )
    );
  }
}
