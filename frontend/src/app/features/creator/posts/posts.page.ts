import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonCard,
  IonCardContent,
  IonBadge,
  IonFab,
  IonFabButton,
  IonIcon,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonSkeletonText,
  IonItemSliding,
  IonItem,
  IonItemOptions,
  IonItemOption,
  RefresherCustomEvent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  addOutline,
  trashOutline,
  createOutline,
  eyeOutline,
  heartOutline,
} from 'ionicons/icons';
import { CreatorService, ToastService } from '../../../core';
import { CreatorPostResponse, PostStatus } from '../../../models';

type FilterTab = 'all' | 'draft' | 'pending' | 'published' | 'rejected';

@Component({
  selector: 'app-creator-posts',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonCard,
    IonCardContent,
    IonBadge,
    IonFab,
    IonFabButton,
    IonIcon,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonSkeletonText,
    IonItemSliding,
    IonItem,
    IonItemOptions,
    IonItemOption,
  ],
  templateUrl: './posts.page.html',
  styleUrls: ['./posts.page.scss'],
})
export class PostsPage implements OnInit {
  private readonly creatorService = inject(CreatorService);
  private readonly toast = inject(ToastService);

  readonly posts = signal<CreatorPostResponse[]>([]);
  readonly isLoading = signal(false);
  readonly activeTab = signal<FilterTab>('all');
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);

  constructor() {
    addIcons({ addOutline, trashOutline, createOutline, eyeOutline, heartOutline });
  }

  ngOnInit(): void {
    this.loadPosts(true);
  }

  onTabChange(tab: FilterTab): void {
    if (this.activeTab() !== tab) {
      this.activeTab.set(tab);
      this.loadPosts(true);
    }
  }

  loadPosts(refresh = false): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }

    this.isLoading.set(true);

    const status = this.getStatusFilter();
    this.creatorService
      .getPosts({ status, page: this.currentPage(), pageSize: 20 })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.posts.set(r.data);
            } else {
              this.posts.update((current) => [...current, ...r.data!]);
            }
            if (r.meta) {
              this.hasMore.set(r.meta.hasNextPage);
            }
          }
        },
        error: () => this.toast.error('Failed to load posts'),
        complete: () => this.isLoading.set(false),
      });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadPosts(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadPosts();
    setTimeout(() => event.target.complete(), 1000);
  }

  deletePost(post: CreatorPostResponse): void {
    this.creatorService.deletePost(post.id).subscribe({
      next: (r) => {
        if (r.success) {
          this.posts.update((posts) => posts.filter((p) => p.id !== post.id));
          this.toast.success('Post deleted');
        } else {
          this.toast.error(r.error?.message || 'Failed to delete post');
        }
      },
      error: () => this.toast.error('Failed to delete post'),
    });
  }

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'medium';
      case PostStatus.PendingReview: return 'warning';
      case PostStatus.Published: return 'success';
      case PostStatus.Rejected: return 'danger';
      default: return 'medium';
    }
  }

  getStatusLabel(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'Draft';
      case PostStatus.PendingReview: return 'Pending';
      case PostStatus.Published: return 'Published';
      case PostStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }

  getEmptyMessage(): string {
    switch (this.activeTab()) {
      case 'draft': return 'No draft posts';
      case 'pending': return 'No posts pending review';
      case 'published': return 'No published posts yet';
      case 'rejected': return 'No rejected posts';
      default: return 'No posts yet. Create your first post!';
    }
  }

  private getStatusFilter(): PostStatus | undefined {
    switch (this.activeTab()) {
      case 'draft': return PostStatus.Draft;
      case 'pending': return PostStatus.PendingReview;
      case 'published': return PostStatus.Published;
      case 'rejected': return PostStatus.Rejected;
      default: return undefined;
    }
  }
}
