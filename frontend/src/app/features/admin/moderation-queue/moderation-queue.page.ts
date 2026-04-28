import { Component, OnInit, inject, signal } from '@angular/core';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
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
  IonCard,
  IonCardContent,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  checkmarkOutline,
  closeOutline,
  chevronDownOutline,
  chevronUpOutline,
  imageOutline,
  videocamOutline,
  flagOutline,
  alertCircleOutline,
  eyeOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import {
  ModerationQueueItemResponse,
  PostReviewData,
  PostStatus,
  MediaType,
} from '../../../models';
import { SkeletonListComponent } from '../../../shared/components/skeleton-list';
import { PostReviewModalComponent } from '../components/post-review-modal';

@Component({
  selector: 'app-moderation-queue',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
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
    IonCard,
    IonCardContent,
    SkeletonListComponent,
    PostReviewModalComponent,
  ],
  templateUrl: './moderation-queue.page.html',
  styleUrls: ['./moderation-queue.page.scss'],
})
export class ModerationQueuePage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly items = signal<ModerationQueueItemResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isActioning = signal(false);
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);
  readonly expandedId = signal<string | null>(null);

  readonly PostStatus = PostStatus;
  readonly MediaType = MediaType;

  readonly reviewModalOpen = signal(false);
  readonly reviewItem = signal<PostReviewData | null>(null);

  constructor() {
    addIcons({
      checkmarkOutline,
      closeOutline,
      chevronDownOutline,
      chevronUpOutline,
      imageOutline,
      videocamOutline,
      flagOutline,
      alertCircleOutline,
      eyeOutline,
    });
  }

  ngOnInit(): void {
    this.loadItems(true);
  }

  loadItems(refresh = false, event?: CustomEvent): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }
    this.isLoading.set(true);

    this.adminService
      .getModerationQueue(this.currentPage(), 20)
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.items.set(r.data);
            } else {
              this.items.update((items) => [...items, ...r.data!]);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => {
          this.toast.error('Failed to load moderation queue');
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
        complete: () => {
          this.isLoading.set(false);
          (event?.target as any)?.complete();
        },
      });
  }

  onRefresh(event: CustomEvent): void {
    this.loadItems(true, event);
  }

  loadMore(event: CustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadItems(false, event);
  }

  toggleExpand(id: string): void {
    this.expandedId.update((current) => (current === id ? null : id));
  }

  async onApprove(item: ModerationQueueItemResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Approve Post',
      message: `Approve "${item.title || 'Untitled'}" by ${item.author.displayName}?`,
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Approve',
          handler: () => {
            this.moderatePost(item.id, true);
          },
        },
      ],
    });
    await alert.present();
  }

  async onReject(item: ModerationQueueItemResponse): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Reject Post',
      message: `Reject "${item.title || 'Untitled'}" by ${item.author.displayName}?`,
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
            this.moderatePost(item.id, false, data.reason.trim());
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
            this.items.update((items) => items.filter((i) => i.id !== postId));
            this.expandedId.set(null);
          } else {
            this.toast.error(r.error?.message || 'Failed to moderate post');
          }
        },
        error: () => this.toast.error('Failed to moderate post'),
        complete: () => this.isActioning.set(false),
      });
  }

  openReview(item: ModerationQueueItemResponse): void {
    this.reviewItem.set(item);
    this.reviewModalOpen.set(true);
  }

  onPostModerated(postId: string): void {
    this.items.update((items) => items.filter((i) => i.id !== postId));
    this.reviewModalOpen.set(false);
    this.reviewItem.set(null);
    this.expandedId.set(null);
  }

  onReviewDismissed(): void {
    this.reviewModalOpen.set(false);
    this.reviewItem.set(null);
  }

  getPriorityLevel(priority: number): string {
    if (priority > 75) return 'high';
    if (priority > 25) return 'medium';
    return 'low';
  }

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.PendingReview:
        return 'warning';
      case PostStatus.Flagged:
        return 'danger';
      case PostStatus.Published:
        return 'success';
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
