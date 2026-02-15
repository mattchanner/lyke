import { Component, inject, signal, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonIcon,
  IonSpinner,
  IonGrid,
  IonRow,
  IonCol,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { globeOutline } from 'ionicons/icons';
import { CommerceService } from '../../../core/services/commerce.service';
import { ProductResponse, RetailerResponse } from '../../../models';
import { ProductGridCardComponent } from '../components/product-grid-card';

@Component({
  selector: 'app-retailer-storefront',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonIcon,
    IonSpinner,
    IonGrid,
    IonRow,
    IonCol,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    ProductGridCardComponent,
  ],
  templateUrl: './retailer-storefront.page.html',
  styleUrls: ['./retailer-storefront.page.scss'],
})
export class RetailerStorefrontPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly commerce = inject(CommerceService);

  readonly retailer = signal<RetailerResponse | null>(null);
  readonly products = signal<ProductResponse[]>([]);
  readonly isLoading = signal(false);
  readonly currentPage = signal(1);
  readonly hasMore = signal(false);

  constructor() {
    addIcons({ globeOutline });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadRetailer(id);
      this.loadProducts(id);
    }
  }

  private loadRetailer(id: string): void {
    this.commerce.getRetailers().subscribe((retailers) => {
      const found = retailers.find((r) => r.id === id);
      if (found) this.retailer.set(found);
    });
  }

  loadProducts(retailerId?: string): void {
    const id = retailerId ?? this.route.snapshot.paramMap.get('id');
    if (!id) return;

    this.isLoading.set(true);
    this.commerce
      .getRetailerProductsRaw(id, this.currentPage(), 20)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            if (this.currentPage() === 1) {
              this.products.set(response.data);
            } else {
              this.products.update((prev) => [...prev, ...response.data!]);
            }
            this.hasMore.set(response.meta?.hasNextPage ?? false);
          }
        },
        complete: () => this.isLoading.set(false),
      });
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadProducts();
    setTimeout(() => event.target.complete(), 1000);
  }

  openWebsite(): void {
    const url = this.retailer()?.websiteUrl;
    if (url) {
      this.commerce.openProductUrl(url);
    }
  }
}
