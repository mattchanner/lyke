import { Component, inject, signal, computed, OnInit, DestroyRef } from '@angular/core';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonAvatar,
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
import { ApiService, AuthService, PostEngagementService, AnalyticsService } from '../../../core';
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
    IonAvatar,
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
  private readonly analytics = inject(AnalyticsService);
  private readonly destroyRef = inject(DestroyRef);
  private readonly engagementService = inject(PostEngagementService);
  private readonly route = inject(ActivatedRoute);
  readonly auth = inject(AuthService);

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
  readonly filterCreatorId = signal<string | null>(null);
  readonly filterCreatorName = signal<string | null>(null);
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
    if (this.filterCreatorId()) count++;
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
    this.route.queryParams
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((params) => {
        const creatorId = params['creatorId'] ?? null;
        if (creatorId !== this.filterCreatorId()) {
          this.filterCreatorId.set(creatorId);
          this.filterCreatorName.set(params['creatorName'] ?? null);
          this.loadFeed(true);
        }
      });

    this.loadFeed();

    this.engagementService.engagementChanged
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((change) => {
        this.posts.update((posts) =>
          posts.map((p) => {
            if (p.id !== change.postId) return p;
            if (change.type === 'like') {
              return {
                ...p,
                isLiked: change.state,
                engagements: {
                  ...p.engagements,
                  likes: p.engagements.likes + (change.state ? 1 : -1),
                },
              };
            }
            return {
              ...p,
              isSaved: change.state,
              engagements: {
                ...p.engagements,
                saves: p.engagements.saves + (change.state ? 1 : -1),
              },
            };
          })
        );
      });
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
      creatorId: this.filterCreatorId() ?? undefined,
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

  onPostLiked(_event: { postId: string; liked: boolean }): void {
    // Post-card already applies the optimistic update via direct mutation,
    // so no signal update is needed here. Duplicating it would double-count.
  }

  onPostSaved(_event: { postId: string; saved: boolean }): void {
    // Post-card already applies the optimistic update via direct mutation,
    // so no signal update is needed here. Duplicating it would double-count.
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
    this.analytics.track('feed.filter', {
      category: filters.category ?? '',
      retailerId: filters.retailerId ?? '',
      fitTagCount: String(filters.fitTagIds.length),
    });
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

  removeCreator(): void {
    this.filterCreatorId.set(null);
    this.filterCreatorName.set(null);
    this.loadFeed(true);
  }

  removeFitTag(tagId: number): void {
    this.filterFitTagIds.update(ids => ids.filter(id => id !== tagId));
    this.loadFeed(true);
  }
}
