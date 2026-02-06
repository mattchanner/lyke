import { Component, inject, signal, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonCard,
  IonCardContent,
  IonBadge,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonItem,
  IonList,
  IonIcon,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  IonInfiniteScroll,
  IonInfiniteScrollContent,
  RefresherCustomEvent,
  InfiniteScrollCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  cashOutline,
  linkOutline,
  megaphoneOutline,
} from 'ionicons/icons';
import { CreatorService, ToastService } from '../../../core';
import {
  EarningsSummaryResponse,
  EarningDetailResponse,
  EarningStatus,
  EarningType,
} from '../../../models';

type EarningTab = 'all' | 'pending' | 'confirmed' | 'paid';

@Component({
  selector: 'app-earnings',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonCard,
    IonCardContent,
    IonBadge,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonItem,
    IonList,
    IonIcon,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
    IonInfiniteScroll,
    IonInfiniteScrollContent,
  ],
  templateUrl: './earnings.page.html',
  styleUrls: ['./earnings.page.scss'],
})
export class EarningsPage implements OnInit {
  private readonly creatorService = inject(CreatorService);
  private readonly toast = inject(ToastService);

  readonly EarningType = EarningType;

  readonly summary = signal<EarningsSummaryResponse | null>(null);
  readonly earnings = signal<EarningDetailResponse[]>([]);
  readonly isLoading = signal(true);
  readonly activeTab = signal<EarningTab>('all');
  readonly currentPage = signal(1);
  readonly hasMore = signal(true);

  constructor() {
    addIcons({ cashOutline, linkOutline, megaphoneOutline });
  }

  ngOnInit(): void {
    this.loadSummary();
    this.loadHistory(true);
  }

  loadSummary(): void {
    this.creatorService.getEarnings().subscribe((s) => this.summary.set(s));
  }

  loadHistory(refresh = false): void {
    if (refresh) {
      this.currentPage.set(1);
      this.hasMore.set(true);
    }

    this.isLoading.set(true);

    const status = this.getStatusFilter();
    this.creatorService
      .getEarningsHistory({ status, page: this.currentPage(), pageSize: 20 })
      .subscribe({
        next: (r) => {
          if (r.success && r.data) {
            if (refresh) {
              this.earnings.set(r.data);
            } else {
              this.earnings.update((current) => [...current, ...r.data!]);
            }
            if (r.meta) {
              this.hasMore.set(r.meta.hasNextPage);
            }
          }
        },
        error: () => this.toast.error('Failed to load earnings'),
        complete: () => this.isLoading.set(false),
      });
  }

  onTabChange(tab: EarningTab): void {
    if (this.activeTab() !== tab) {
      this.activeTab.set(tab);
      this.loadHistory(true);
    }
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadSummary();
    this.loadHistory(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  loadMore(event: InfiniteScrollCustomEvent): void {
    this.currentPage.update((p) => p + 1);
    this.loadHistory();
    setTimeout(() => event.target.complete(), 1000);
  }

  getEarningTypeLabel(type: EarningType): string {
    return type === EarningType.Affiliate ? 'Affiliate' : 'Sponsored';
  }

  getEarningTypeColor(type: EarningType): string {
    return type === EarningType.Affiliate ? 'primary' : 'tertiary';
  }

  getStatusLabel(status: EarningStatus): string {
    switch (status) {
      case EarningStatus.Pending: return 'Pending';
      case EarningStatus.Confirmed: return 'Confirmed';
      case EarningStatus.Paid: return 'Paid';
      default: return 'Unknown';
    }
  }

  getStatusColor(status: EarningStatus): string {
    switch (status) {
      case EarningStatus.Pending: return 'warning';
      case EarningStatus.Confirmed: return 'success';
      case EarningStatus.Paid: return 'primary';
      default: return 'medium';
    }
  }

  private getStatusFilter(): EarningStatus | undefined {
    switch (this.activeTab()) {
      case 'pending': return EarningStatus.Pending;
      case 'confirmed': return EarningStatus.Confirmed;
      case 'paid': return EarningStatus.Paid;
      default: return undefined;
    }
  }
}
