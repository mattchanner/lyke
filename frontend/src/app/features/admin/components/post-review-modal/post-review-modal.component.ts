import { Component, inject, input, output, signal, computed, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonIcon,
  IonContent,
  IonFooter,
  IonBadge,
  IonCard,
  IonCardContent,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  closeOutline,
  checkmarkOutline,
  flagOutline,
  alertCircleOutline,
  checkmarkCircle,
} from 'ionicons/icons';

import { AdminService, ToastService } from '../../../../core/services';
import { PostReviewData, PostStatus, MediaType } from '../../../../models';
import { MediaCarouselComponent, MediaItem } from '../../../../shared/components/media-carousel';

@Component({
  selector: 'app-post-review-modal',
  standalone: true,
  imports: [
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonContent,
    IonFooter,
    IonBadge,
    IonCard,
    IonCardContent,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-review-modal.component.html',
  styleUrls: ['./post-review-modal.component.scss'],
})
export class PostReviewModalComponent {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly isOpen = input(false);
  readonly post = input<PostReviewData | null>(null);

  readonly dismissed = output<void>();
  readonly moderated = output<string>();

  readonly isActioning = signal(false);
  readonly PostStatus = PostStatus;

  readonly mediaItems = computed<MediaItem[]>(() => {
    const p = this.post();
    if (!p) return [];
    return p.mediaUrls.map((url, i) => ({
      url,
      type: p.mediaType ?? MediaType.Image,
      thumbnailUrl: p.thumbnailUrls?.[i],
    }));
  });

  readonly canModerate = computed(() => {
    const p = this.post();
    if (!p) return false;
    return (
      p.status === PostStatus.PendingReview ||
      p.status === PostStatus.Flagged ||
      p.status === PostStatus.Published
    );
  });

  constructor() {
    addIcons({
      closeOutline,
      checkmarkOutline,
      flagOutline,
      alertCircleOutline,
      checkmarkCircle,
    });
  }

  close(): void {
    this.dismissed.emit();
  }

  async onApprove(): Promise<void> {
    const p = this.post();
    if (!p) return;

    const alert = await this.alertController.create({
      header: 'Approve Post',
      message: `Approve "${p.title || 'Untitled'}" by ${p.author.displayName}?`,
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Approve',
          handler: () => {
            this.moderatePost(p.id, true);
          },
        },
      ],
    });
    await alert.present();
  }

  async onReject(): Promise<void> {
    const p = this.post();
    if (!p) return;

    const alert = await this.alertController.create({
      header: 'Reject Post',
      message: `Reject "${p.title || 'Untitled'}" by ${p.author.displayName}?`,
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
            this.moderatePost(p.id, false, data.reason.trim());
            return true;
          },
        },
      ],
    });
    await alert.present();
  }

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Published:
        return 'success';
      case PostStatus.PendingReview:
        return 'warning';
      case PostStatus.Rejected:
        return 'danger';
      case PostStatus.Flagged:
        return 'danger';
      case PostStatus.Removed:
        return 'dark';
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

  private moderatePost(postId: string, approve: boolean, rejectionReason?: string): void {
    this.isActioning.set(true);
    this.adminService
      .moderatePost(postId, { approve, rejectionReason })
      .subscribe({
        next: (r) => {
          if (r.success) {
            this.toast.success(approve ? 'Post approved' : 'Post rejected');
            this.moderated.emit(postId);
            this.dismissed.emit();
          } else {
            this.toast.error(r.error?.message || 'Failed to moderate post');
          }
        },
        error: () => this.toast.error('Failed to moderate post'),
        complete: () => this.isActioning.set(false),
      });
  }
}
