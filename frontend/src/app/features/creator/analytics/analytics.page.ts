import {
  Component,
  inject,
  signal,
  computed,
  OnInit,
  OnDestroy,
  ViewChild,
  ElementRef,
  effect,
} from '@angular/core';
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
  IonSegment,
  IonSegmentButton,
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
import { CreatorService } from '../../../core';
import { CreatorAnalyticsResponse, DailyMetrics } from '../../../models';

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
type ChartMetric = 'views' | 'likes' | 'clicks' | 'earnings';

const METRIC_COLORS: Record<ChartMetric, { line: string; fill: string }> = {
  views: { line: '#0E3A3B', fill: 'rgba(14, 58, 59, 0.15)' },
  likes: { line: '#c94c4c', fill: 'rgba(201, 76, 76, 0.15)' },
  clicks: { line: '#d4a843', fill: 'rgba(212, 168, 67, 0.15)' },
  earnings: { line: '#5b8c5a', fill: 'rgba(91, 140, 90, 0.15)' },
};

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
    IonSegment,
    IonSegmentButton,
  ],
  templateUrl: './analytics.page.html',
  styleUrls: ['./analytics.page.scss'],
})
export class AnalyticsPage implements OnInit, OnDestroy {
  private readonly creatorService = inject(CreatorService);

  @ViewChild('chartCanvas', { static: false })
  chartCanvas!: ElementRef<HTMLCanvasElement>;

  private chart: Chart | null = null;

  readonly analytics = signal<CreatorAnalyticsResponse | null>(null);
  readonly isLoading = signal(true);
  readonly dateRange = signal<DateRange>('30d');
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
    addIcons({
      eyeOutline,
      heartOutline,
      bookmarkOutline,
      handLeftOutline,
      cashOutline,
      trendingUpOutline,
    });

    effect(() => {
      const a = this.analytics();
      const metric = this.chartMetric();
      if (a?.dailyMetrics?.length) {
        // Defer to next tick so the canvas is available
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

  setDateRange(range: DateRange): void {
    if (this.dateRange() !== range) {
      this.dateRange.set(range);
      this.loadAnalytics();
    }
  }

  setChartMetric(metric: ChartMetric): void {
    this.chartMetric.set(metric);
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

  private renderChart(data: DailyMetrics[], metric: ChartMetric): void {
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
                if (metric === 'earnings') {
                  return `$${val.toFixed(2)}`;
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
                if (metric === 'earnings') return `$${num}`;
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
