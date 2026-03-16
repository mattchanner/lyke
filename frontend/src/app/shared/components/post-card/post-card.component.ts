import { Component, Input, Output, EventEmitter, inject, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import {
  IonCard,
  IonCardContent,
  IonButton,
  IonIcon,
  IonChip,
  IonLabel,
  IonAvatar,
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
  ellipsisVerticalOutline,
  flagOutline,
} from 'ionicons/icons';
import { HttpContext } from '@angular/common/http';
import { ApiService, ToastService, AnalyticsService, FollowService } from '../../../core';
import { SUPPRESS_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
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
    IonButton,
    IonIcon,
    IonChip,
    IonLabel,
    IonAvatar,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-card.component.html',
  styleUrls: ['./post-card.component.scss'],
})
export class PostCardComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly analytics = inject(AnalyticsService);
  private readonly actionSheetCtrl = inject(ActionSheetController);
  private readonly alertCtrl = inject(AlertController);
  private readonly router = inject(Router);
  readonly followService = inject(FollowService);

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
      ellipsisVerticalOutline,
      flagOutline,
    });
  }

  get mediaItems(): MediaItem[] {
    return this.post.mediaUrls.map((url, i) => ({
      url,
      type: this.post.mediaType ?? MediaType.Image,
      thumbnailUrl: this.post.thumbnailUrls[i],
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

  toggleFollow(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    if (this.followService.isFollowing(this.post.creator.id)) {
      this.followService.unfollow(this.post.creator.id);
    } else {
      this.followService.follow(this.post.creator.id);
    }
  }

  navigateToCreator(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.router.navigate(['/profile/creator', this.post.creator.id]);
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
        this.analytics.track(
          newLikedState ? 'post.like' : 'post.unlike',
          { postId: this.post.id, source: 'feed' }, this.post.id, 'Post'
        );
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
        this.analytics.track(
          newSavedState ? 'post.save' : 'post.unsave',
          { postId: this.post.id, source: 'feed' }, this.post.id, 'Post'
        );
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

    this.analytics.track('post.share', { postId: this.post.id, source: 'feed' }, this.post.id, 'Post');

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

    const reasonAlert = await this.alertCtrl.create({
      header: 'Report Post',
      message: 'Why are you reporting this post?',
      inputs: Object.entries(reasonLabels).map(([value, label], i) => ({
        name: 'reason',
        type: 'radio' as const,
        label,
        value,
        checked: i === 0,
      })),
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Next',
          handler: (reason: string) => {
            if (!reason) return false;
            this.showDetailsDialog(reason as ReportReason);
            return true;
          },
        },
      ],
    });
    await reasonAlert.present();
  }

  private async showDetailsDialog(reason: ReportReason): Promise<void> {
    const detailsAlert = await this.alertCtrl.create({
      header: 'Additional Details',
      message: 'Any additional details? (optional)',
      inputs: [
        {
          name: 'additionalDetails',
          type: 'textarea' as const,
          placeholder: 'Describe the issue...',
        },
      ],
      buttons: [
        {
          text: 'Skip',
          handler: () => {
            this.submitReport({ reason });
          },
        },
        {
          text: 'Submit',
          handler: (data) => {
            this.submitReport({
              reason,
              additionalDetails: data.additionalDetails?.trim() || undefined,
            });
          },
        },
      ],
    });
    await detailsAlert.present();
  }

  private submitReport(body: CreateReportRequest): void {
    const context = new HttpContext().set(SUPPRESS_ERROR_TOAST, true);
    this.api.post('posts', `${this.post.id}/report`, body, { context }).subscribe({
      next: () => {
        this.toast.success('Report submitted');
        this.reported.emit(this.post.id);
      },
      error: (err) => {
        if (err.details?.['Report']?.some((d: string) => d.includes('already'))) {
          this.toast.error("You've already reported this post");
        } else {
          this.toast.error('Failed to submit report');
        }
      },
    });
  }
}
