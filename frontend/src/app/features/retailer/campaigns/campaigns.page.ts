import { Component, OnInit, signal, inject } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  IonSegment,
  IonSegmentButton,
  IonFab,
  IonFabButton,
  IonIcon,
  RefresherCustomEvent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { addOutline, megaphoneOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import { CampaignResponse } from '../../../models';

type FilterTab = 'all' | 'active';

@Component({
  selector: 'app-campaigns',
  standalone: true,
  imports: [
    DecimalPipe,
    DatePipe,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonList,
    IonItem,
    IonLabel,
    IonBadge,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
    IonSegment,
    IonSegmentButton,
    IonFab,
    IonFabButton,
    IonIcon,
  ],
  templateUrl: './campaigns.page.html',
  styleUrls: ['./campaigns.page.scss'],
})
export class CampaignsPage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly toast = inject(ToastService);

  readonly campaigns = signal<CampaignResponse[]>([]);
  readonly isLoading = signal(true);
  readonly hasMore = signal(true);
  readonly currentPage = signal(1);
  readonly activeFilter = signal<FilterTab>('all');

  constructor() {
    addIcons({ addOutline, megaphoneOutline });
  }

  ngOnInit(): void {
    this.loadCampaigns();
  }

  loadCampaigns(append = false): void {
    if (!append) {
      this.isLoading.set(true);
      this.currentPage.set(1);
    }

    this.retailerService
      .getCampaigns({
        isActive: this.activeFilter() === 'active' ? true : undefined,
        page: this.currentPage(),
        pageSize: 20,
      })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (append) {
              this.campaigns.update((prev) => [...prev, ...r.data!]);
            } else {
              this.campaigns.set(r.data);
            }
            this.hasMore.set(r.meta?.hasNextPage ?? false);
          }
        },
        error: () => this.toast.error('Failed to load campaigns'),
        complete: () => this.isLoading.set(false),
      });
  }

  onFilterChange(event: CustomEvent): void {
    this.activeFilter.set(event.detail.value as FilterTab);
    this.loadCampaigns();
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadCampaigns();
    setTimeout(() => event.target.complete(), 1000);
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadCampaigns(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  getStatusColor(status: string): string {
    switch (status) {
      case 'Active':
        return 'success';
      case 'Scheduled':
        return 'warning';
      case 'Ended':
        return 'medium';
      case 'Paused':
        return 'danger';
      default:
        return 'medium';
    }
  }
}
