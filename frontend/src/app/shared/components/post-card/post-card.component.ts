import { Component, Input, Output, EventEmitter, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonCard,
  IonCardContent,
  IonCardHeader,
  IonCardTitle,
  IonButton,
  IonIcon,
  IonChip,
  IonLabel,
  IonAvatar,
  IonBadge,
  ActionSheetController,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  heartOutline,
  heart,
  bookmarkOutline,
  bookmark,
  shareSocialOutline,
  checkmarkCircle,
  eyeOutline,
  ellipsisVertical,
  flagOutline,
} from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core';
import { FeedPostResponse, PostProductSummaryResponse, EngagementType, MediaType, ReportReason, CreateReportRequest } from '../../../models';
import { MediaCarouselComponent, MediaItem } from '../media-carousel';

@Component({
  selector: 'app-post-card',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonCard,
    IonCardContent,
    IonCardHeader,
    IonCardTitle,
    IonButton,
    IonIcon,
    IonChip,
    IonLabel,
    IonAvatar,
    IonBadge,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.scss'],
})
export class PostCardComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly actionSheetCtrl = inject(ActionSheetController);
  private readonly alertCtrl = inject(AlertController);

  @Input({ required: true }) post!: FeedPostResponse;
  @Output() liked = new EventEmitter<{ postId: string; liked: boolean }>();
  @Output() saved = new EventEmitter<{ postId: string; saved: boolean }>();
  @Output() productTapped = new EventEmitter<{ postId: string; product: PostProductSummaryResponse }>();
  @Output() reported = new EventEmitter<string>();

  constructor() {
    addIcons({
      heartOutline,
      heart,
      bookmarkOutline,
      bookmark,
      shareSocialOutline,
      checkmarkCircle,
      eyeOutline,
      ellipsisVertical,
      flagOutline,
    });
  }

  get mediaItems(): MediaItem[] {
    return this.post.mediaUrls.map((url, i) => ({
      url,
      type: this.post.mediaType ?? MediaType.Image,
      thumbnailUrl: this.post.thumbnailUrls?.[i],
    }));
  }

  formatCount(count: number): string {
    if (count >= 1000000) {
      return (count / 1000000).toFixed(1) + 'M';
    }
    if (count >= 1000) {
      return (count / 1000).toFixed(1) + 'K';
    }
    return count.toString();
  }

  toggleLike(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    const newLikedState = !this.post.isLiked;
    const body = { type: EngagementType.Like };

    const request$ = newLikedState
      ? this.api.post(`posts`, `${this.post.id}/engage`, body)
      : this.api.delete(`posts`, `${this.post.id}/engage`, body);

    // Optimistic update
    this.post.isLiked = newLikedState;
    this.post.engagements.likes += newLikedState ? 1 : -1;

    request$.subscribe({
      next: () => {
        this.liked.emit({ postId: this.post.id, liked: newLikedState });
      },
      error: () => {
        // Revert on failure
        this.post.isLiked = !newLikedState;
        this.post.engagements.likes += newLikedState ? -1 : 1;
        this.toast.error('Failed to update like');
      },
    });
  }

  toggleSave(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    const newSavedState = !this.post.isSaved;
    const body = { type: EngagementType.Save };

    const request$ = newSavedState
      ? this.api.post(`posts`, `${this.post.id}/engage`, body)
      : this.api.delete(`posts`, `${this.post.id}/engage`, body);

    // Optimistic update
    this.post.isSaved = newSavedState;
    this.post.engagements.saves += newSavedState ? 1 : -1;

    request$.subscribe({
      next: () => {
        this.saved.emit({ postId: this.post.id, saved: newSavedState });
        this.toast.success(newSavedState ? 'Saved!' : 'Removed from saved');
      },
      error: () => {
        // Revert on failure
        this.post.isSaved = !newSavedState;
        this.post.engagements.saves += newSavedState ? -1 : 1;
        this.toast.error('Failed to save post');
      },
    });
  }

  onProductTap(event: Event, product: PostProductSummaryResponse): void {
    event.stopPropagation();
    event.preventDefault();
    this.productTapped.emit({ postId: this.post.id, product });
  }

  share(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    if (navigator.share) {
      navigator.share({
        title: this.post.title || 'Check out this outfit on LYKE',
        url: window.location.origin + '/feed/post/' + this.post.id,
      });
    } else {
      navigator.clipboard.writeText(
        window.location.origin + '/feed/post/' + this.post.id
      );
      this.toast.success('Link copied!');
    }
  }

  async reportPost(event: Event): Promise<void> {
    event.stopPropagation();
    event.preventDefault();

    const actionSheet = await this.actionSheetCtrl.create({
      buttons: [
        {
          text: 'Report Post',
          role: 'destructive',
          icon: 'flag-outline',
          handler: () => {
            this.showReportDialog();
          },
        },
        { text: 'Cancel', role: 'cancel' },
      ],
    });
    await actionSheet.present();
  }

  private async showReportDialog(): Promise<void> {
    const reasonLabels: Record<ReportReason, string> = {
      [ReportReason.InappropriateContent]: 'Inappropriate Content',
      [ReportReason.Spam]: 'Spam',
      [ReportReason.MisleadingProductTag]: 'Misleading Product Tag',
      [ReportReason.Copyright]: 'Copyright Violation',
      [ReportReason.HateSpeech]: 'Hate Speech',
      [ReportReason.Other]: 'Other',
    };

    const alert = await this.alertCtrl.create({
      header: 'Report Post',
      message: 'Why are you reporting this post?',
      inputs: [
        ...Object.entries(reasonLabels).map(([value, label], i) => ({
          name: 'reason',
          type: 'radio' as const,
          label,
          value,
          checked: i === 0,
        })),
        {
          name: 'additionalDetails',
          type: 'textarea' as const,
          placeholder: 'Additional details (optional)...',
        },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Submit',
          handler: (data) => {
            const body: CreateReportRequest = {
              reason: data.reason || data,
              additionalDetails: data.additionalDetails?.trim() || undefined,
            };
            this.submitReport(body);
          },
        },
      ],
    });
    await alert.present();
  }

  private submitReport(body: CreateReportRequest): void {
    this.api.post('posts', `${this.post.id}/report`, body).subscribe({
      next: () => {
        this.toast.success('Report submitted');
        this.reported.emit(this.post.id);
      },
      error: (err) => {
        if (err.status === 409) {
          this.toast.error("You've already reported this post");
        } else {
          this.toast.error('Failed to submit report');
        }
      },
    });
  }
}
