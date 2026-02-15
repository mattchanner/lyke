import { Component, OnInit, signal, inject } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonButton,
  IonIcon,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonChip,
  IonList,
  IonItem,
  IonLabel,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { downloadOutline } from 'ionicons/icons';
import { RetailerService, ToastService } from '../../../core';
import {
  RetailerAnalyticsResponse,
  RetailerAnalyticsRequest,
} from '../../../models';

type DateRange = '7d' | '30d' | '90d';

@Component({
  selector: 'app-retailer-analytics',
  standalone: true,
  imports: [
    DecimalPipe,
    DatePipe,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonButton,
    IonIcon,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonChip,
    IonList,
    IonItem,
    IonLabel,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
  ],
  templateUrl: './retailer-analytics.page.html',
  styleUrls: ['./retailer-analytics.page.scss'],
})
export class RetailerAnalyticsPage implements OnInit {
  private readonly retailerService = inject(RetailerService);
  private readonly toast = inject(ToastService);

  readonly analytics = signal<RetailerAnalyticsResponse | null>(null);
  readonly isLoading = signal(true);
  readonly selectedRange = signal<DateRange>('30d');

  constructor() {
    addIcons({ downloadOutline });
  }

  ngOnInit(): void {
    this.loadAnalytics();
  }

  selectRange(range: DateRange): void {
    this.selectedRange.set(range);
    this.loadAnalytics();
  }

  private getDateRequest(): RetailerAnalyticsRequest {
    const end = new Date();
    const start = new Date();
    const days =
      this.selectedRange() === '7d'
        ? 7
        : this.selectedRange() === '30d'
          ? 30
          : 90;
    start.setDate(start.getDate() - days);
    return {
      startDate: start.toISOString().split('T')[0],
      endDate: end.toISOString().split('T')[0],
    };
  }

  loadAnalytics(refresh = false): void {
    if (!refresh) this.isLoading.set(true);
    this.retailerService.getAnalytics(this.getDateRequest()).subscribe((data) => {
      this.analytics.set(data);
      this.isLoading.set(false);
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadAnalytics(true);
    setTimeout(() => event.target.complete(), 1000);
  }

  exportCsv(): void {
    this.retailerService.exportAnalyticsCsv(this.getDateRequest()).subscribe({
      next: (blob) => {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'analytics-export.csv';
        a.click();
        URL.revokeObjectURL(url);
      },
      error: () => this.toast.error('Export failed'),
    });
  }
}
