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
  templateUrl: './feed-home.page.html',
  styleUrls: ['./feed-home.page.scss'],
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
