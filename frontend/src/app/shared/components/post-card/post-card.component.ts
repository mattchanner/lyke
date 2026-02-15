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
import { FeedPostResponse, PostProductSummaryResponse, EngagementType, MediaType } from '../../../models';
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

  @Input({ required: true }) post!: FeedPostResponse;
  @Output() liked = new EventEmitter<{ postId: string; liked: boolean }>();
  @Output() saved = new EventEmitter<{ postId: string; saved: boolean }>();
  @Output() productTapped = new EventEmitter<{ postId: string; product: PostProductSummaryResponse }>();

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

    this.api
      .post(`posts`, `${this.post.id}/engage`, {
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
      .post(`posts`, `${this.post.id}/engage`, {
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
}
