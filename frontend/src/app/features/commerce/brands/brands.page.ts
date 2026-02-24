import { Component, inject, signal, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonSearchbar,
  IonButtons,
  IonMenuButton,
  IonSpinner,
  IonAvatar,
  IonNote,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { ApiService } from '../../../core';
import { BrandResponse } from '../../../models';

@Component({
  selector: 'app-brands',
  standalone: true,
  imports: [
    FormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonSearchbar,
    IonButtons,
    IonMenuButton,
    IonSpinner,
    IonAvatar,
    IonNote,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
  ],
  templateUrl: './brands.page.html',
  styleUrls: ['./brands.page.scss'],
})
export class BrandsPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);

  readonly brands = signal<BrandResponse[]>([]);
  readonly isLoading = signal(false);
  readonly hasMore = signal(true);
  readonly currentPage = signal(1);

  searchQuery = '';
  private searchTimeout: ReturnType<typeof setTimeout> | null = null;

  ngOnInit(): void {
    this.loadBrands(true);
  }

  onSearchChange(): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }
    this.searchTimeout = setTimeout(() => {
      this.loadBrands(true);
    }, 300);
  }

  loadBrands(refresh = false): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }

    this.isLoading.set(true);

    const params: Record<string, string | number> = {
      page: this.currentPage(),
      pageSize: 20,
    };

    if (this.searchQuery.trim()) {
      params['search'] = this.searchQuery.trim();
    }

    this.api.get<BrandResponse[]>('brands', '', params).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          if (refresh) {
            this.brands.set(response.data);
          } else {
            this.brands.update((current) => [...current, ...response.data!]);
          }

          if (response.meta) {
            this.hasMore.set(response.meta.hasNextPage);
          }
        }
      },
      error: () => {
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadBrands();
    setTimeout(() => event.target.complete(), 1000);
  }

  onBrandTap(brand: BrandResponse): void {
    this.router.navigate(['/shop/retailers', brand.id]);
  }
}
