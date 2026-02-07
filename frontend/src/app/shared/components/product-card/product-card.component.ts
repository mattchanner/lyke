import { Component, inject, Input } from '@angular/core';
import {
  IonCard,
  IonCardContent,
  IonChip,
  IonLabel,
  IonButton,
  IonIcon,
  IonSpinner,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { cartOutline } from 'ionicons/icons';
import { CommerceService } from '../../../core/services/commerce.service';
import { PostProductDetailResponse, FitRating } from '../../../models';

@Component({
  selector: 'app-product-card',
  standalone: true,
  imports: [
    IonCard,
    IonCardContent,
    IonChip,
    IonLabel,
    IonButton,
    IonIcon,
    IonSpinner,
  ],
  templateUrl: './product-card.component.html',
  styleUrls: ['./product-card.component.scss'],
})
export class ProductCardComponent {
  private readonly commerce = inject(CommerceService);

  @Input({ required: true }) product!: PostProductDetailResponse;
  @Input({ required: true }) postId!: string;
  @Input() source: 'feed' | 'post_detail' | 'search' | 'similar_posts' = 'post_detail';

  readonly isTracking = this.commerce.isTracking;

  constructor() {
    addIcons({ cartOutline });
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

  shopProduct(): void {
    this.commerce.trackAndShop(this.postId, this.product.id, {
      source: this.source,
    });
  }
}
