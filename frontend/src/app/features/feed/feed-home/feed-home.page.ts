import { Component, inject, signal, computed, OnInit } from '@angular/core';
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
  IonMenuButton,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonSpinner,
  IonChip,
  IonLabel,
  IonBadge,
  InfiniteScrollCustomEvent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  searchOutline,
  bookmarkOutline,
  personCircleOutline,
  filterOutline,
  closeCircle,
} from 'ionicons/icons';
import { ApiService, AuthService } from '../../../core';
import { FeedPostResponse, FeedSortBy } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';
import { SkeletonPostCardComponent } from '../../../shared/components/loading-skeleton';
import { FeedFilterModalComponent, FeedFilters } from '../components/feed-filter-modal/feed-filter-modal.component';

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
    IonMenuButton,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonSpinner,
    IonChip,
    IonLabel,
    IonBadge,
    PostCardComponent,
    SkeletonPostCardComponent,
    FeedFilterModalComponent,
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

  // Filter state
  readonly isFilterOpen = signal(false);
  readonly filterCategory = signal<string | null>(null);
  readonly filterRetailerId = signal<string | null>(null);
  readonly filterRetailerName = signal<string | null>(null);
  readonly filterFitTagIds = signal<number[]>([]);

  readonly currentFilters = computed<FeedFilters>(() => ({
    category: this.filterCategory(),
    retailerId: this.filterRetailerId(),
    fitTagIds: this.filterFitTagIds(),
  }));

  readonly activeFilterCount = computed(() => {
    let count = 0;
    if (this.filterCategory()) count++;
    if (this.filterRetailerId()) count++;
    count += this.filterFitTagIds().length;
    return count;
  });

  constructor() {
    addIcons({
      searchOutline,
      bookmarkOutline,
      personCircleOutline,
      filterOutline,
      closeCircle,
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
      category: this.filterCategory() ?? undefined,
      retailerId: this.filterRetailerId() ?? undefined,
      fitTagIds: this.filterFitTagIds().length > 0 ? this.filterFitTagIds() : undefined,
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

  openFilter(): void {
    this.isFilterOpen.set(true);
  }

  closeFilter(): void {
    this.isFilterOpen.set(false);
  }

  onFiltersChanged(filters: FeedFilters): void {
    this.filterCategory.set(filters.category);
    this.filterRetailerId.set(filters.retailerId);
    this.filterFitTagIds.set(filters.fitTagIds);
    this.loadFeed(true);
  }

  removeCategory(): void {
    this.filterCategory.set(null);
    this.loadFeed(true);
  }

  removeRetailer(): void {
    this.filterRetailerId.set(null);
    this.filterRetailerName.set(null);
    this.loadFeed(true);
  }

  removeFitTag(tagId: number): void {
    this.filterFitTagIds.update(ids => ids.filter(id => id !== tagId));
    this.loadFeed(true);
  }
}
