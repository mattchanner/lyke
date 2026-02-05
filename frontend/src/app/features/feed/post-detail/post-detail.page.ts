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
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/feed"></ion-back-button>
        </ion-buttons>
        <ion-title>Post</ion-title>
        <ion-buttons slot="end">
          <ion-button (click)="share()">
            <ion-icon name="share-social-outline" slot="icon-only"></ion-icon>
          </ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      @if (isLoading()) {
        <app-skeleton-post-detail></app-skeleton-post-detail>
      } @else if (post()) {
        <div class="post-detail">
          <div class="media-section">
            @for (url of post()!.mediaUrls; track url; let i = $index) {
              <img [src]="url" [alt]="'Image ' + (i + 1)" />
            }
          </div>

          <div class="content-section">
            <div class="creator-row">
              <ion-avatar>
                <img src="assets/default-avatar.svg" [alt]="post()!.creator.displayName" />
              </ion-avatar>
              <div class="creator-info">
                <span class="creator-name">
                  {{ post()!.creator.displayName }}
                  @if (post()!.creator.isVerified) {
                    <ion-icon name="checkmark-circle" color="primary"></ion-icon>
                  }
                </span>
                @if (post()!.creator.bodyProfile; as bp) {
                  <span class="body-info">
                    {{ bp.heightRange }} ·
                    {{ bp.weightRange }} ·
                    {{ bp.bodyTypeName }}
                  </span>
                }
              </div>
            </div>

            <div class="engagement-actions">
              <ion-button fill="clear" (click)="toggleLike()">
                <ion-icon
                  [name]="post()!.isLiked ? 'heart' : 'heart-outline'"
                  [color]="post()!.isLiked ? 'danger' : 'medium'"
                  slot="start"
                ></ion-icon>
                {{ post()!.engagements.likes }}
              </ion-button>
              <ion-button fill="clear" (click)="toggleSave()">
                <ion-icon
                  [name]="post()!.isSaved ? 'bookmark' : 'bookmark-outline'"
                  [color]="post()!.isSaved ? 'primary' : 'medium'"
                  slot="start"
                ></ion-icon>
                {{ post()!.engagements.saves }}
              </ion-button>
            </div>

            @if (post()!.title || post()!.description) {
              <div class="post-text">
                @if (post()!.title) {
                  <h2>{{ post()!.title }}</h2>
                }
                @if (post()!.description) {
                  <p>{{ post()!.description }}</p>
                }
              </div>
            }

            <h3>Products in this outfit</h3>
            @for (product of post()!.products; track product.id) {
              <ion-card>
                <ion-card-content>
                  <div class="product-row">
                    @if (product.productImageUrls.length > 0) {
                      <img
                        [src]="product.productImageUrls[0]"
                        [alt]="product.productName"
                        class="product-image"
                      />
                    }
                    <div class="product-info">
                      <span class="product-name">{{ product.productName }}</span>
                      <span class="product-retailer">{{ product.retailerName }}</span>
                      <span class="product-price">
                        {{ product.currency }} {{ product.price.toFixed(2) }}
                      </span>
                    </div>
                  </div>

                  <div class="fit-info">
                    <ion-chip>
                      <ion-label>Size worn: {{ product.sizeWorn }}</ion-label>
                    </ion-chip>
                    @if (product.fitRating !== null) {
                      <ion-chip [color]="getFitRatingColor(product.fitRating)">
                        <ion-label>{{ getFitRatingLabel(product.fitRating) }}</ion-label>
                      </ion-chip>
                    }
                  </div>

                  @if (product.fitNotes) {
                    <p class="fit-notes">"{{ product.fitNotes }}"</p>
                  }

                  @if (product.fitTags.length > 0) {
                    <div class="fit-tags">
                      @for (tag of product.fitTags; track tag.id) {
                        <ion-chip size="small">
                          <ion-label>{{ tag.name }}</ion-label>
                        </ion-chip>
                      }
                    </div>
                  }

                  <ion-button expand="block" (click)="shopProduct(product.productUrl)">
                    <ion-icon name="cart-outline" slot="start"></ion-icon>
                    Shop Now
                  </ion-button>
                </ion-card-content>
              </ion-card>
            }
          </div>
        </div>
      }
    </ion-content>
  `,
  styles: [`
    .media-section {
      img {
        width: 100%;
        display: block;
      }
    }

    .content-section {
      padding: 1rem;
    }

    .creator-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 1rem;

      ion-avatar {
        width: 48px;
        height: 48px;
      }

      .creator-name {
        display: flex;
        align-items: center;
        gap: 0.25rem;
        font-weight: 600;
      }

      .body-info {
        font-size: 0.85rem;
        color: var(--ion-color-medium);
      }
    }

    .engagement-actions {
      display: flex;
      gap: 0.5rem;
      margin-bottom: 1rem;
      padding-bottom: 1rem;
      border-bottom: 1px solid var(--ion-color-light);
    }

    .post-text {
      margin-bottom: 1.5rem;

      h2 {
        font-size: 1.25rem;
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--ion-color-dark);
        line-height: 1.5;
      }
    }

    h3 {
      font-size: 1.1rem;
      margin-bottom: 0.75rem;
    }

    ion-card {
      margin: 0 0 1rem 0;
    }

    .product-row {
      display: flex;
      gap: 1rem;
      margin-bottom: 0.75rem;

      .product-image {
        width: 80px;
        height: 80px;
        object-fit: cover;
        border-radius: 8px;
      }

      .product-info {
        display: flex;
        flex-direction: column;

        .product-name {
          font-weight: 600;
        }

        .product-retailer {
          font-size: 0.85rem;
          color: var(--ion-color-medium);
        }

        .product-price {
          font-weight: 600;
          color: var(--ion-color-primary);
        }
      }
    }

    .fit-info {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 0.75rem;
    }

    .fit-notes {
      font-style: italic;
      color: var(--ion-color-medium);
      margin-bottom: 0.75rem;
    }

    .fit-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.25rem;
      margin-bottom: 0.75rem;
    }
  `],
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
