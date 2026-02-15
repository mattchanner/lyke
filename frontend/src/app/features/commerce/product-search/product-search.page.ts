import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonButton,
  IonIcon,
  IonSearchbar,
  IonSelect,
  IonSelectOption,
  IonChip,
  IonLabel,
  IonSpinner,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonGrid,
  IonRow,
  IonCol,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { storefrontOutline } from 'ionicons/icons';
import { CommerceService } from '../../../core/services/commerce.service';
import { ProductResponse, RetailerResponse } from '../../../models';
import { ProductGridCardComponent } from '../components/product-grid-card';

@Component({
  selector: 'app-product-search',
  standalone: true,
  imports: [
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonButton,
    IonIcon,
    IonSearchbar,
    IonSelect,
    IonSelectOption,
    IonChip,
    IonLabel,
    IonSpinner,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonGrid,
    IonRow,
    IonCol,
    ProductGridCardComponent,
  ],
  templateUrl: './product-search.page.html',
  styleUrls: ['./product-search.page.scss'],
})
export class ProductSearchPage implements OnInit {
  private readonly commerce = inject(CommerceService);

  readonly query = signal('');
  readonly retailerFilter = signal<string | undefined>(undefined);
  readonly categoryFilter = signal<string | undefined>(undefined);
  readonly products = signal<ProductResponse[]>([]);
  readonly retailers = signal<RetailerResponse[]>([]);
  readonly isLoading = signal(false);
  readonly currentPage = signal(1);
  readonly hasMore = signal(false);

  readonly categories = computed(() => {
    const cats = new Set(this.products().map((p) => p.category));
    return [...cats].sort();
  });

  constructor() {
    addIcons({ storefrontOutline });
  }

  ngOnInit(): void {
    this.commerce.getRetailers().subscribe((r) => this.retailers.set(r));
    this.search();
  }

  onSearchChange(event: CustomEvent): void {
    this.query.set(event.detail.value ?? '');
    this.resetAndSearch();
  }

  onRetailerChange(event: CustomEvent): void {
    this.retailerFilter.set(event.detail.value || undefined);
    this.resetAndSearch();
  }

  selectCategory(cat: string): void {
    this.categoryFilter.set(this.categoryFilter() === cat ? undefined : cat);
    this.resetAndSearch();
  }

  private resetAndSearch(): void {
    this.currentPage.set(1);
    this.hasMore.set(false);
    this.products.set([]);
    this.search();
  }

  search(): void {
    this.isLoading.set(true);
    this.commerce
      .searchProductsRaw(
        this.query(),
        this.retailerFilter(),
        this.categoryFilter(),
        this.currentPage(),
        20
      )
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
    this.search();
    setTimeout(() => event.target.complete(), 1000);
  }
}
