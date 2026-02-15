import { Component, OnInit, inject, signal } from '@angular/core';
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
  IonList,
  IonItem,
  IonThumbnail,
  IonBadge,
  IonButton,
  IonIcon,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonRefresher,
  IonRefresherContent,
  IonNote,
  AlertController,
  IonCard,
  IonCardContent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  checkmarkOutline,
  closeOutline,
  chevronDownOutline,
  chevronUpOutline,
  imageOutline,
  videocamOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { PendingPostResponse, PostStatus, MediaType } from '../../../models';
import { SkeletonListComponent } from '../../../shared/components/skeleton-list';

type TabFilter = PostStatus | 'all';

@Component({
  selector: 'app-post-moderation',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonList,
    IonItem,
    IonThumbnail,
    IonBadge,
    IonButton,
    IonIcon,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonRefresher,
    IonRefresherContent,
    IonNote,
    IonCard,
    IonCardContent,
    SkeletonListComponent,
  ],
  templateUrl: './post-moderation.page.html',
  styleUrls: ['./post-moderation.page.scss'],
})
export class PostModerationPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly posts = signal<PendingPostResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isActioning = signal(false);
  readonly activeTab = signal<TabFilter>(PostStatus.PendingReview);
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);
  readonly expandedPostId = signal<string | null>(null);

  readonly PostStatus = PostStatus;
  readonly MediaType = MediaType;

  constructor() {
    addIcons({
      checkmarkOutline,
      closeOutline,
      chevronDownOutline,
      chevronUpOutline,
      imageOutline,
      videocamOutline,
    });
  }

  ngOnInit(): void {
    this.loadPosts(true);
  }

  loadPosts(refresh = false, event?: CustomEvent): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }
    this.isLoading.set(true);

    const tab = this.activeTab();
    const status = tab === 'all' ? undefined : (tab as PostStatus);
    this.adminService
      .getPendingPosts({ status, page: this.currentPage(), pageSize: 20 })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.posts.set(r.data);
            } else {
              this.posts.update((p) => [...p, ...r.data!]);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => {
          this.toast.error('Failed to load posts');
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
        complete: () => {
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
      });
  }

  onTabChange(event: CustomEvent): void {
    this.activeTab.set(event.detail.value as TabFilter);
    this.expandedPostId.set(null);
    this.loadPosts(true);
  }

  onRefresh(event: CustomEvent): void {
    this.loadPosts(true, event);
  }

  loadMore(event: CustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadPosts(false, event);
  }

  toggleExpand(postId: string): void {
    this.expandedPostId.update((id) => (id === postId ? null : postId));
  }

  async onApprove(post: PendingPostResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Approve Post',
      message: `Approve "${post.title || 'Untitled'}" by ${post.creator.displayName}?`,
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Approve',
          handler: () => {
            this.moderatePost(post.id, true);
          },
        },
      ],
    });
    await alert.present();
  }

  async onReject(post: PendingPostResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Reject Post',
      message: `Reject "${post.title || 'Untitled'}" by ${post.creator.displayName}?`,
      inputs: [
        {
          name: 'reason',
          type: 'textarea',
          placeholder: 'Rejection reason (required)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Reject',
          role: 'destructive',
          handler: (data) => {
            if (!data.reason?.trim()) {
              this.toast.error('Rejection reason is required');
              return false;
            }
            this.moderatePost(post.id, false, data.reason.trim());
            return true;
          },
        },
      ],
    });
    await alert.present();
  }

  private moderatePost(postId: string, approve: boolean, rejectionReason?: string): void {
    this.isActioning.set(true);
    this.adminService
      .moderatePost(postId, { approve, rejectionReason })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success(approve ? 'Post approved' : 'Post rejected');
            this.posts.update((posts) => posts.filter((p) => p.id !== postId));
            this.expandedPostId.set(null);
          } else {
            this.toast.error(r.error?.message || 'Failed to moderate post');
          }
        },
        error: () => this.toast.error('Failed to moderate post'),
        complete: () => this.isActioning.set(false),
      });
  }

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Published:
        return 'success';
      case PostStatus.PendingReview:
        return 'warning';
      case PostStatus.Rejected:
        return 'danger';
      case PostStatus.Draft:
        return 'medium';
      default:
        return 'medium';
    }
  }

  formatDate(dateString: string | null): string {
    if (!dateString) return 'N/A';
    return new Date(dateString).toLocaleDateString('en-US', {
      month: 'short',
      day: 'numeric',
      year: 'numeric',
    });
  }
}
