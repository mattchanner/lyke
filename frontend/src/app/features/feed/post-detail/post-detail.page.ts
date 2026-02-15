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
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  heartOutline,
  heart,
  bookmarkOutline,
  bookmark,
  shareSocialOutline,
  checkmarkCircle,
} from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core';
import { PostDetailResponse, EngagementType, MediaType } from '../../../models';
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

    this.api
      .post(`posts`, `${currentPost.id}/engage`, {
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
      .post(`posts`, `${currentPost.id}/engage`, {
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

}
