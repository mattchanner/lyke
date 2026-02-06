import { Component, inject, signal, OnInit } from '@angular/core';
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
  IonCard,
  IonCardContent,
  IonChip,
  IonLabel,
  IonAvatar,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  heartOutline,
  heart,
  bookmarkOutline,
  bookmark,
  shareSocialOutline,
  checkmarkCircle,
  cartOutline,
} from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core';
import { PostDetailResponse, EngagementType, FitRating } from '../../../models';
import { SkeletonPostDetailComponent } from '../../../shared/components/loading-skeleton';

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
    IonCard,
    IonCardContent,
    IonChip,
    IonLabel,
    IonAvatar,
    SkeletonPostDetailComponent,
  ],
  templateUrl: './post-detail.page.html',
  styleUrls: ['./post-detail.page.scss'],
})
export class PostDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly post = signal<PostDetailResponse | null>(null);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({
      heartOutline,
      heart,
      bookmarkOutline,
      bookmark,
      shareSocialOutline,
      checkmarkCircle,
      cartOutline,
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

    this.api.get<PostDetailResponse>('feed', `posts/${id}`).subscribe({
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

    this.api
      .post(`feed`, `posts/${currentPost.id}/engage`, {
        type: EngagementType.Like,
      })
      .subscribe({
        next: () => {
          this.post.update((p) =>
            p
              ? {
                  ...p,
                  isLiked: !p.isLiked,
                  engagements: {
                    ...p.engagements,
                    likes: p.engagements.likes + (p.isLiked ? -1 : 1),
                  },
                }
              : null
          );
        },
      });
  }

  toggleSave(): void {
    const currentPost = this.post();
    if (!currentPost) return;

    this.api
      .post(`feed`, `posts/${currentPost.id}/engage`, {
        type: EngagementType.Save,
      })
      .subscribe({
        next: () => {
          const newSaved = !currentPost.isSaved;
          this.post.update((p) =>
            p
              ? {
                  ...p,
                  isSaved: newSaved,
                  engagements: {
                    ...p.engagements,
                    saves: p.engagements.saves + (newSaved ? 1 : -1),
                  },
                }
              : null
          );
          this.toast.success(newSaved ? 'Saved!' : 'Removed from saved');
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

  getFitRatingLabel(rating: FitRating): string {
    const labels: Record<FitRating, string> = {
      [FitRating.TooSmall]: 'Too Small',
      [FitRating.SlightlySmall]: 'Slightly Small',
      [FitRating.TrueToSize]: 'True to Size',
      [FitRating.SlightlyLarge]: 'Slightly Large',
      [FitRating.TooLarge]: 'Too Large',
    };
    return labels[rating];
  }

  getFitRatingColor(rating: FitRating): string {
    if (rating === FitRating.TrueToSize) return 'success';
    if (rating === FitRating.SlightlySmall || rating === FitRating.SlightlyLarge)
      return 'warning';
    return 'danger';
  }

  shopProduct(url: string): void {
    window.open(url, '_blank');
  }
}
