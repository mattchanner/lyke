import {
  Component,
  OnInit,
  OnDestroy,
  signal,
  computed,
  inject,
  ViewChild,
  ElementRef,
  effect,
} from '@angular/core';
import { DecimalPipe } from '@angular/common';
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
  IonSegment,
  IonSegmentButton,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { downloadOutline, trendingUpOutline } from 'ionicons/icons';
import {
  Chart,
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  Tooltip,
} from 'chart.js';
import { RetailerService, ToastService } from '../../../core';
import {
  RetailerAnalyticsResponse,
  RetailerAnalyticsRequest,
  RetailerDailyMetrics,
} from '../../../models';

Chart.register(
  LineController,
  LineElement,
  PointElement,
  LinearScale,
  CategoryScale,
  Filler,
  Tooltip,
);

type DateRange = '7d' | '30d' | '90d';
type ChartMetric = 'views' | 'clicks' | 'conversions' | 'revenue';

const METRIC_COLORS: Record<ChartMetric, { line: string; fill: string }> = {
  views: { line: '#0E3A3B', fill: 'rgba(14, 58, 59, 0.15)' },
  clicks: { line: '#d4a843', fill: 'rgba(212, 168, 67, 0.15)' },
  conversions: { line: '#7b68ae', fill: 'rgba(123, 104, 174, 0.15)' },
  revenue: { line: '#5b8c5a', fill: 'rgba(91, 140, 90, 0.15)' },
};

@Component({
  selector: 'app-retailer-analytics',
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
    IonSegment,
    IonSegmentButton,
  ],
  templateUrl: './retailer-analytics.page.html',
  styleUrls: ['./retailer-analytics.page.scss'],
})
export class RetailerAnalyticsPage implements OnInit, OnDestroy {
  private readonly retailerService = inject(RetailerService);
  private readonly toast = inject(ToastService);

  @ViewChild('chartCanvas', { static: false })
  chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;

  readonly analytics = signal<RetailerAnalyticsResponse | null>(null);
  readonly isLoading = signal(true);
  readonly selectedRange = signal<DateRange>('30d');
  readonly chartMetric = signal<ChartMetric>('views');

  readonly chartTotal = computed(() => {
    const a = this.analytics();
    if (!a) return 0;
    const metric = this.chartMetric();
    return a.dailyMetrics.reduce(
      (sum, d) => sum + (d[metric] as number),
      0,
    );
  });

  constructor() {
    addIcons({ downloadOutline, trendingUpOutline });

    effect(() => {
      const a = this.analytics();
      const metric = this.chartMetric();
      if (a?.dailyMetrics?.length) {
        setTimeout(() => this.renderChart(a.dailyMetrics, metric), 0);
      }
    });
  }

  ngOnInit(): void {
    this.loadAnalytics();
  }

  ngOnDestroy(): void {
    this.chart?.destroy();
  }

  selectRange(range: DateRange): void {
    this.selectedRange.set(range);
    this.loadAnalytics();
  }

  setChartMetric(metric: ChartMetric): void {
    this.chartMetric.set(metric);
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

  private renderChart(data: RetailerDailyMetrics[], metric: ChartMetric): void {
    if (!this.chartCanvas?.nativeElement) return;

    this.chart?.destroy();

    const ctx = this.chartCanvas.nativeElement.getContext('2d');
    if (!ctx) return;

    const colors = METRIC_COLORS[metric];
    const labels = data.map((d) => {
      const date = new Date(d.date);
      return `${date.getMonth() + 1}/${date.getDate()}`;
    });
    const values = data.map((d) => d[metric] as number);

    this.chart = new Chart(ctx, {
      type: 'line',
      data: {
        labels,
        datasets: [
          {
            data: values,
            borderColor: colors.line,
            backgroundColor: colors.fill,
            borderWidth: 2,
            fill: true,
            tension: 0.35,
            pointRadius: data.length <= 14 ? 4 : 0,
            pointHoverRadius: 6,
            pointBackgroundColor: colors.line,
            pointBorderColor: '#fff',
            pointBorderWidth: 2,
          },
        ],
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          tooltip: {
            backgroundColor: '#1d1d1f',
            titleColor: '#e8e6e1',
            bodyColor: '#e8e6e1',
            cornerRadius: 8,
            padding: 10,
            displayColors: false,
            callbacks: {
              label: (context) => {
                const val = context.parsed.y ?? 0;
                if (metric === 'revenue') {
                  return `${val.toFixed(2)}`;
                }
                return val.toLocaleString();
              },
            },
          },
        },
        scales: {
          x: {
            grid: { display: false },
            ticks: {
              color: '#8e8e93',
              font: { size: 11, family: 'Plus Jakarta Sans' },
              maxTicksLimit: 7,
            },
            border: { display: false },
          },
          y: {
            beginAtZero: true,
            grid: { color: 'rgba(0, 0, 0, 0.04)' },
            ticks: {
              color: '#8e8e93',
              font: { size: 11, family: 'Plus Jakarta Sans' },
              maxTicksLimit: 5,
              callback: (value) => {
                const num = value as number;
                if (metric === 'revenue') return `${num}`;
                if (num >= 1000) return `${(num / 1000).toFixed(1)}k`;
                return num.toString();
              },
            },
            border: { display: false },
          },
        },
        interaction: {
          intersect: false,
          mode: 'index',
        },
      },
    });
  }
}
