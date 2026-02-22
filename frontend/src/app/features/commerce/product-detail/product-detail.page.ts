import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DecimalPipe } from '@angular/common';
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
  IonChip,
  IonLabel,
} from '@ionic/angular/standalone';
import { CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { addIcons } from 'ionicons';
import { shareSocialOutline, cartOutline } from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core';
import { CommerceService } from '../../../core/services/commerce.service';
import { ProductResponse, FeedPostResponse, MediaType } from '../../../models';
import {
  MediaCarouselComponent,
  MediaItem,
} from '../../../shared/components/media-carousel';
import { PostCardComponent } from '../../../shared/components/post-card';

@Component({
  selector: 'app-product-detail',
  standalone: true,
  imports: [
    DecimalPipe,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonButton,
    IonIcon,
    IonChip,
    IonLabel,
    MediaCarouselComponent,
    PostCardComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './product-detail.page.html',
  styleUrls: ['./product-detail.page.scss'],
})
export class ProductDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  private readonly commerce = inject(CommerceService);
  private readonly toast = inject(ToastService);

  readonly product = signal<ProductResponse | null>(null);
  readonly relatedPosts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isBrowsing = this.commerce.isBrowsing;

  readonly mediaItems = computed<MediaItem[]>(() => {
    const p = this.product();
    if (!p) return [];
    return p.imageUrls.map((url) => ({
      url,
      type: MediaType.Image,
    }));
  });

  constructor() {
    addIcons({ shareSocialOutline, cartOutline });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadProduct(id);
    }
  }

  loadProduct(id: string): void {
    this.isLoading.set(true);

    this.commerce.getProduct(id).subscribe({
      next: (product) => {
        if (product) {
          this.product.set(product);
          this.loadRelatedPosts(product.id);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  private loadRelatedPosts(productId: string): void {
    this.api
      .get<FeedPostResponse[]>('commerce', `products/${productId}/posts`)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.relatedPosts.set(response.data);
          }
        },
      });
  }

  shopProduct(): void {
    const p = this.product();
    if (p) {
      this.commerce.openProductUrl(p.productUrl);
    }
  }

  share(): void {
    const p = this.product();
    if (!p) return;

    const url = `${window.location.origin}/product/${p.id}`;
    if (navigator.share) {
      navigator.share({
        title: p.name,
        url,
      });
    } else {
      navigator.clipboard.writeText(url);
      this.toast.success('Link copied!');
    }
  }
}
