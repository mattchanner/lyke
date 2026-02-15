import { Component, OnInit, signal, inject } from '@angular/core';
import { DecimalPipe, PercentPipe } from '@angular/common';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonSegment,
  IonSegmentButton,
  IonLabel,
  IonList,
  IonItem,
  IonBadge,
  IonChip,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  IonProgressBar,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { RetailerService } from '../../../core';
import {
  FitInsightsResponse,
  BodyProfileInsightsResponse,
  ProductFitSummary,
} from '../../../models';

type InsightTab = 'fit' | 'body';

@Component({
  selector: 'app-insights',
  standalone: true,
  imports: [
    DecimalPipe,
    PercentPipe,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButtons,
    IonBackButton,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonSegment,
    IonSegmentButton,
    IonLabel,
    IonList,
    IonItem,
    IonBadge,
    IonChip,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
    IonProgressBar,
  ],
  templateUrl: './insights.page.html',
  styleUrls: ['./insights.page.scss'],
})
export class InsightsPage implements OnInit {
  private readonly retailerService = inject(RetailerService);

  readonly activeTab = signal<InsightTab>('fit');
  readonly fitInsights = signal<FitInsightsResponse | null>(null);
  readonly bodyInsights = signal<BodyProfileInsightsResponse | null>(null);
  readonly isLoading = signal(true);
  readonly expandedProduct = signal<string | null>(null);

  ngOnInit(): void {
    this.loadData();
  }

  segmentChanged(event: CustomEvent): void {
    this.activeTab.set(event.detail.value as InsightTab);
  }

  toggleProduct(productId: string): void {
    this.expandedProduct.set(
      this.expandedProduct() === productId ? null : productId
    );
  }

  fitColor(recommendation: string): string {
    const r = recommendation.toLowerCase();
    if (r.includes('true to size') || r.includes('good')) return 'success';
    if (r.includes('small')) return 'warning';
    if (r.includes('large')) return 'tertiary';
    return 'medium';
  }

  loadData(refresh = false): void {
    if (!refresh) this.isLoading.set(true);
    this.retailerService.getFitInsights().subscribe((data) => {
      this.fitInsights.set(data);
      this.isLoading.set(false);
    });
    this.retailerService.getBodyProfileInsights().subscribe((data) => {
      this.bodyInsights.set(data);
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadData(true);
    setTimeout(() => event.target.complete(), 1000);
  }
}
