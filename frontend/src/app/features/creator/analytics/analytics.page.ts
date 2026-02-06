import { Component, inject, signal, computed, OnInit } from '@angular/core';
import { DatePipe, DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonChip,
  IonLabel,
  IonIcon,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  eyeOutline,
  heartOutline,
  bookmarkOutline,
  handLeftOutline,
  cashOutline,
  trendingUpOutline,
} from 'ionicons/icons';
import { CreatorService } from '../../../core';
import { CreatorAnalyticsResponse } from '../../../models';

type DateRange = '7d' | '30d' | '90d';

@Component({
  selector: 'app-analytics',
  standalone: true,
  imports: [
    DatePipe,
    DecimalPipe,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonChip,
    IonLabel,
    IonIcon,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
  ],
  templateUrl: './analytics.page.html',
  styleUrls: ['./analytics.page.scss'],
})
export class AnalyticsPage implements OnInit {
  private readonly creatorService = inject(CreatorService);

  readonly analytics = signal<CreatorAnalyticsResponse | null>(null);
  readonly isLoading = signal(true);
  readonly dateRange = signal<DateRange>('30d');

  readonly maxDailyViews = computed(() => {
    const data = this.analytics()?.dailyMetrics;
    if (!data?.length) return 1;
    return Math.max(...data.map((d) => d.views), 1);
  });

  constructor() {
    addIcons({
      eyeOutline,
      heartOutline,
      bookmarkOutline,
      handLeftOutline,
      cashOutline,
      trendingUpOutline,
    });
  }

  ngOnInit(): void {
    this.loadAnalytics();
  }

  setDateRange(range: DateRange): void {
    if (this.dateRange() !== range) {
      this.dateRange.set(range);
      this.loadAnalytics();
    }
  }

  loadAnalytics(): void {
    this.isLoading.set(true);
    const { startDate, endDate } = this.getDateRange();
    this.creatorService.getAnalytics({ startDate, endDate }).subscribe((a) => {
      this.analytics.set(a);
      this.isLoading.set(false);
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadAnalytics();
    setTimeout(() => event.target.complete(), 1000);
  }

  getBarWidth(views: number): number {
    return (views / this.maxDailyViews()) * 100;
  }

  private getDateRange(): { startDate: string; endDate: string } {
    const end = new Date();
    const start = new Date();

    switch (this.dateRange()) {
      case '7d': start.setDate(end.getDate() - 7); break;
      case '30d': start.setDate(end.getDate() - 30); break;
      case '90d': start.setDate(end.getDate() - 90); break;
    }

    return {
      startDate: start.toISOString().split('T')[0],
      endDate: end.toISOString().split('T')[0],
    };
  }
}
