import { Component, inject, signal, computed, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonButton,
  IonIcon,
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
  ellipsisVertical,
  flagOutline,
} from 'ionicons/icons';
import { HttpContext } from '@angular/common/http';
import { ApiService, ToastService, PostEngagementService } from '../../../core';
import { SUPPRESS_ERROR_TOAST } from '../../../core/interceptors/error.interceptor';
import { PostDetailResponse, EngagementType, MediaType, ReportReason, CreateReportRequest } from '../../../models';
import { SkeletonPostDetailComponent } from '../../../shared/components/loading-skeleton';
import { ShopTheLookComponent } from '../../../shared/components/shop-the-look';
import { MediaCarouselComponent, MediaItem } from '../../../shared/components/media-carousel';

@Component({
  selector: 'app-post-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonButton,
    IonIcon,
    IonAvatar,
    SkeletonPostDetailComponent,
    ShopTheLookComponent,
    MediaCarouselComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './post-detail.page.html',
  styleUrls: ['./post-detail.page.scss'],
})
export class PostDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);
  private readonly actionSheetCtrl = inject(ActionSheetController);
  private readonly alertCtrl = inject(AlertController);
  private readonly engagement = inject(PostEngagementService);

  readonly post = signal<PostDetailResponse | null>(null);
  readonly isLoading = signal(false);

  readonly mediaItems = computed<MediaItem[]>(() => {
    const p = this.post();
    if (!p) return [];
    return p.mediaUrls.map((url, i) => ({
      url,
      type: p.mediaType ?? MediaType.Image,
      thumbnailUrl: p.thumbnailUrls?.[i],
    }));
  });

  constructor() {
    addIcons({
      heartOutline,
      heart,
      bookmarkOutline,
      bookmark,
      shareSocialOutline,
      checkmarkCircle,
      ellipsisVertical,
      flagOutline,
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadPost(id);
    }
  }

  loadPost(id: string): void {
    this.isLoading.set(true);

    this.api.get<PostDetailResponse>('posts', `${id}`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.post.set(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  toggleLike(): void {
    const currentPost = this.post();
    if (!currentPost) return;

    const newLikedState = !currentPost.isLiked;
    const body = { type: EngagementType.Like };

    const request$ = newLikedState
      ? this.api.post(`posts`, `${currentPost.id}/engage`, body)
      : this.api.delete(`posts`, `${currentPost.id}/engage`, body);

    request$.subscribe({
      next: () => {
        this.post.update((p) =>
          p
            ? {
                ...p,
                isLiked: newLikedState,
                engagements: {
                  ...p.engagements,
                  likes: p.engagements.likes + (newLikedState ? 1 : -1),
                },
              }
            : null
        );
        this.engagement.notify({ postId: currentPost.id, type: 'like', state: newLikedState });
      },
    });
  }

  toggleSave(): void {
    const currentPost = this.post();
    if (!currentPost) return;

    const newSavedState = !currentPost.isSaved;
    const body = { type: EngagementType.Save };

    const request$ = newSavedState
      ? this.api.post(`posts`, `${currentPost.id}/engage`, body)
      : this.api.delete(`posts`, `${currentPost.id}/engage`, body);

    request$.subscribe({
      next: () => {
        this.post.update((p) =>
          p
            ? {
                ...p,
                isSaved: newSavedState,
                engagements: {
                  ...p.engagements,
                  saves: p.engagements.saves + (newSavedState ? 1 : -1),
                },
              }
            : null
        );
        this.engagement.notify({ postId: currentPost.id, type: 'save', state: newSavedState });
        this.toast.success(newSavedState ? 'Saved!' : 'Removed from saved');
      },
    });
  }

  share(): void {
    const currentPost = this.post();
    if (!currentPost) return;

    if (navigator.share) {
      navigator.share({
        title: currentPost.title || 'Check out this outfit on LYKE',
        url: window.location.href,
      });
    } else {
      navigator.clipboard.writeText(window.location.href);
      this.toast.success('Link copied!');
    }
  }

  async reportPost(): Promise<void> {
    const currentPost = this.post();
    if (!currentPost) return;

    const actionSheet = await this.actionSheetCtrl.create({
      buttons: [
        {
          text: 'Report Post',
          role: 'destructive',
          icon: 'flag-outline',
          handler: () => {
            this.showReportDialog(currentPost.id);
          },
        },
        { text: 'Cancel', role: 'cancel' },
      ],
    });
    await actionSheet.present();
  }

  private async showReportDialog(postId: string): Promise<void> {
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
            this.showDetailsDialog(postId, reason as ReportReason);
            return true;
          },
        },
      ],
    });
    await reasonAlert.present();
  }

  private async showDetailsDialog(postId: string, reason: ReportReason): Promise<void> {
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
            this.submitReport(postId, { reason });
          },
        },
        {
          text: 'Submit',
          handler: (data) => {
            this.submitReport(postId, {
              reason,
              additionalDetails: data.additionalDetails?.trim() || undefined,
            });
          },
        },
      ],
    });
    await detailsAlert.present();
  }

  private submitReport(postId: string, body: CreateReportRequest): void {
    const context = new HttpContext().set(SUPPRESS_ERROR_TOAST, true);
    this.api.post('posts', `${postId}/report`, body, { context }).subscribe({
      next: () => {
        this.toast.success('Report submitted');
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
