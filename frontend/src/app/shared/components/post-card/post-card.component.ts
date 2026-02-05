import { Component, Input, Output, EventEmitter, inject } from '@angular/core';
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
} from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core';
import { FeedPostResponse, EngagementType } from '../../../models';

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
  ],
  template: `
    <ion-card [routerLink]="['/feed/post', post.id]">
      <div class="media-container">
        @if (post.mediaUrls.length > 0) {
          <img
            [src]="post.mediaUrls[0]"
            [alt]="post.title || 'Post image'"
            loading="lazy"
          />
        }
        @if (post.mediaUrls.length > 1) {
          <ion-badge class="media-count">+{{ post.mediaUrls.length - 1 }}</ion-badge>
        }
        @if (post.similarityScore >= 0.8) {
          <ion-chip class="similarity-badge" color="success">
            <ion-label>{{ (post.similarityScore * 100).toFixed(0) }}% Match</ion-label>
          </ion-chip>
        }
      </div>

      <ion-card-content>
        <div class="creator-row" (click)="$event.stopPropagation()">
          <ion-avatar>
            <img
              src="assets/default-avatar.svg"
              [alt]="post.creator.displayName"
            />
          </ion-avatar>
          <div class="creator-info">
            <span class="creator-name">
              {{ post.creator.displayName }}
              @if (post.creator.isVerified) {
                <ion-icon name="checkmark-circle" color="primary"></ion-icon>
              }
            </span>
            @if (post.creator.bodyProfile) {
              <span class="body-info">
                {{ post.creator.bodyProfile.heightRange }} ·
                {{ post.creator.bodyProfile.bodyTypeName }}
              </span>
            }
          </div>
        </div>

        @if (post.title) {
          <p class="post-title">{{ post.title }}</p>
        }

        @if (post.products.length > 0) {
          <div class="products-preview">
            @for (product of post.products.slice(0, 2); track product.id) {
              <ion-chip>
                <ion-label>{{ product.productName }} · {{ product.sizeWorn }}</ion-label>
              </ion-chip>
            }
            @if (post.products.length > 2) {
              <ion-chip>
                <ion-label>+{{ post.products.length - 2 }} more</ion-label>
              </ion-chip>
            }
          </div>
        }

        <div class="engagement-row" (click)="$event.stopPropagation()">
          <div class="engagement-stats">
            <span>
              <ion-icon name="eye-outline"></ion-icon>
              {{ formatCount(post.engagements.views) }}
            </span>
            <span>
              <ion-icon [name]="post.isLiked ? 'heart' : 'heart-outline'"></ion-icon>
              {{ formatCount(post.engagements.likes) }}
            </span>
          </div>

          <div class="engagement-actions">
            <ion-button fill="clear" size="small" (click)="toggleLike($event)">
              <ion-icon
                [name]="post.isLiked ? 'heart' : 'heart-outline'"
                [color]="post.isLiked ? 'danger' : 'medium'"
                slot="icon-only"
              ></ion-icon>
            </ion-button>
            <ion-button fill="clear" size="small" (click)="toggleSave($event)">
              <ion-icon
                [name]="post.isSaved ? 'bookmark' : 'bookmark-outline'"
                [color]="post.isSaved ? 'primary' : 'medium'"
                slot="icon-only"
              ></ion-icon>
            </ion-button>
            <ion-button fill="clear" size="small" (click)="share($event)">
              <ion-icon name="share-social-outline" color="medium" slot="icon-only"></ion-icon>
            </ion-button>
          </div>
        </div>
      </ion-card-content>
    </ion-card>
  `,
  styles: [`
    ion-card {
      margin: 0;
      border-radius: 0;
      box-shadow: none;
      background: var(--ion-background-color);
    }

    .media-container {
      position: relative;
      aspect-ratio: 1;
      overflow: hidden;
      background: var(--ion-color-light);

      img {
        width: 100%;
        height: 100%;
        object-fit: cover;
      }

      .media-count {
        position: absolute;
        top: 0.5rem;
        right: 0.5rem;
      }

      .similarity-badge {
        position: absolute;
        bottom: 0.5rem;
        left: 0.5rem;
        --background: rgba(var(--ion-color-success-rgb), 0.9);
        font-size: 0.75rem;
      }
    }

    ion-card-content {
      padding: 0.75rem;
    }

    .creator-row {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin-bottom: 0.5rem;

      ion-avatar {
        width: 36px;
        height: 36px;
      }

      .creator-info {
        display: flex;
        flex-direction: column;

        .creator-name {
          font-weight: 600;
          font-size: 0.9rem;
          display: flex;
          align-items: center;
          gap: 0.25rem;

          ion-icon {
            font-size: 0.9rem;
          }
        }

        .body-info {
          font-size: 0.8rem;
          color: var(--ion-color-medium);
        }
      }
    }

    .post-title {
      margin: 0.5rem 0;
      font-size: 0.9rem;
      line-height: 1.4;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .products-preview {
      display: flex;
      flex-wrap: wrap;
      gap: 0.25rem;
      margin: 0.5rem 0;

      ion-chip {
        height: 24px;
        font-size: 0.75rem;
        --background: var(--ion-color-light);
      }
    }

    .engagement-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.5rem;
      padding-top: 0.5rem;
      border-top: 1px solid var(--ion-color-light);

      .engagement-stats {
        display: flex;
        gap: 1rem;

        span {
          display: flex;
          align-items: center;
          gap: 0.25rem;
          font-size: 0.8rem;
          color: var(--ion-color-medium);

          ion-icon {
            font-size: 0.9rem;
          }
        }
      }

      .engagement-actions {
        display: flex;

        ion-button {
          --padding-start: 0.5rem;
          --padding-end: 0.5rem;
        }
      }
    }
  `],
})
export class PostCardComponent {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  @Input({ required: true }) post!: FeedPostResponse;
  @Output() liked = new EventEmitter<{ postId: string; liked: boolean }>();
  @Output() saved = new EventEmitter<{ postId: string; saved: boolean }>();

  constructor() {
    addIcons({
      heartOutline,
      heart,
      bookmarkOutline,
      bookmark,
      shareSocialOutline,
      checkmarkCircle,
      eyeOutline,
    });
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

    this.api
      .post(`feed`, `posts/${this.post.id}/engage`, {
        type: EngagementType.Like,
      })
      .subscribe({
        next: () => {
          this.liked.emit({ postId: this.post.id, liked: newLikedState });
        },
        error: () => {
          this.toast.error('Failed to update like');
        },
      });
  }

  toggleSave(event: Event): void {
    event.stopPropagation();
    event.preventDefault();

    const newSavedState = !this.post.isSaved;

    this.api
      .post(`feed`, `posts/${this.post.id}/engage`, {
        type: EngagementType.Save,
      })
      .subscribe({
        next: () => {
          this.saved.emit({ postId: this.post.id, saved: newSavedState });
          this.toast.success(newSavedState ? 'Saved!' : 'Removed from saved');
        },
        error: () => {
          this.toast.error('Failed to save post');
        },
      });
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
}
