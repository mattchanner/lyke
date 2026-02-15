import { Component, OnInit, signal, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonButton,
  IonIcon,
  IonSearchbar,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonThumbnail,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonSegment,
  IonSegmentButton,
  RefresherCustomEvent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { cloudUploadOutline, cubeOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import { RetailerProductResponse } from '../../../models';

type FilterTab = 'all' | 'active' | 'inactive';

@Component({
  selector: 'app-retailer-products',
  standalone: true,
  imports: [
    DecimalPipe,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonButton,
    IonIcon,
    IonSearchbar,
    IonList,
    IonItem,
    IonLabel,
    IonBadge,
    IonThumbnail,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonSegment,
    IonSegmentButton,
  ],
  templateUrl: './products.page.html',
  styleUrls: ['./products.page.scss'],
})
export class ProductsPage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly products = signal<RetailerProductResponse[]>([]);
  readonly isLoading = signal(true);
  readonly hasMore = signal(true);
  readonly currentPage = signal(1);
  readonly search = signal('');
  readonly activeFilter = signal<FilterTab>('all');

  constructor() {
    addIcons({ cloudUploadOutline, cubeOutline });
  }

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(append = false): void {
    if (!append) {
      this.isLoading.set(true);
      this.currentPage.set(1);
    }

    const isActive =
      this.activeFilter() === 'active'
        ? true
        : this.activeFilter() === 'inactive'
          ? false
          : undefined;

    this.retailerService
      .getProducts({
        search: this.search() || undefined,
        isActive,
        page: this.currentPage(),
        pageSize: 20,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (append) {
              this.products.update((prev) => [...prev, ...r.data!]);
            } else {
              this.products.set(r.data);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => this.toast.error('Failed to load products'),
        complete: () => this.isLoading.set(false),
      });
  }

  onSearch(event: CustomEvent): void {
    this.search.set(event.detail.value ?? '');
    this.loadProducts();
  }

  onFilterChange(event: CustomEvent): void {
    this.activeFilter.set(event.detail.value as FilterTab);
    this.loadProducts();
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadProducts();
    setTimeout(() => event.target.complete(), 1000);
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadProducts(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  editProduct(product: RetailerProductResponse): void {
    this.router.navigate(['/retailer/products', product.id, 'edit'], {
      state: { product },
    });
  }

  importCsv(): void {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.csv';
    input.onchange = () => {
      const file = input.files?.[0];
      if (!file) return;
      this.retailerService.importProducts(file).subscribe({
        next: (r) => {
          if (r.success && r.data) {
            const d = r.data;
            this.toast.success(
              `Imported: ${d.imported}, Updated: ${d.updated}, Skipped: ${d.skipped}, Failed: ${d.failed}`
            );
            this.loadProducts();
          } else {
            this.toast.error(r.error?.message ?? 'Import failed');
          }
        },
        error: () => this.toast.error('Import failed'),
      });
    };
    input.click();
  }
}
