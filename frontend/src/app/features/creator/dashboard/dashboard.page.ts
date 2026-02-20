import { Component, inject, signal, OnInit } from '@angular/core';
import { DecimalPipe, DatePipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonMenuButton,
  IonList,
  IonItem,
  IonLabel,
  IonIcon,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonBadge,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  imagesOutline,
  addCircleOutline,
  analyticsOutline,
  walletOutline,
  checkmarkCircleOutline,
  eyeOutline,
  heartOutline,
  handLeftOutline,
  cashOutline,
  shieldCheckmarkOutline,
  personCircleOutline,
  newspaperOutline,
} from 'ionicons/icons';
import { CreatorService } from '../../../core';
import {
  CreatorProfileResponse,
  CreatorAnalyticsResponse,
  EarningsSummaryResponse,
  CreatorPostResponse,
  PostStatus,
} from '../../../models';

@Component({
  selector: 'app-creator-dashboard',
  standalone: true,
  imports: [
    DecimalPipe,
    DatePipe,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonButtons,
    IonMenuButton,
    IonList,
    IonItem,
    IonLabel,
    IonIcon,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonBadge,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
  ],
  templateUrl: './dashboard.page.html',
  styleUrls: ['./dashboard.page.scss'],
})
export class DashboardPage implements OnInit {
  private readonly creatorService = inject(CreatorService);

  readonly profile = signal<CreatorProfileResponse | null>(null);
  readonly analytics = signal<CreatorAnalyticsResponse | null>(null);
  readonly earnings = signal<EarningsSummaryResponse | null>(null);
  readonly recentPosts = signal<CreatorPostResponse[]>([]);
  readonly isLoading = signal(true);

  constructor() {
    addIcons({
      imagesOutline,
      addCircleOutline,
      analyticsOutline,
      walletOutline,
      checkmarkCircleOutline,
      eyeOutline,
      heartOutline,
      handLeftOutline,
      cashOutline,
      shieldCheckmarkOutline,
      personCircleOutline,
      newspaperOutline,
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.isLoading.set(true);

    this.creatorService.getProfile().subscribe((p) => this.profile.set(p));
    this.creatorService.getAnalytics().subscribe((a) => this.analytics.set(a));
    this.creatorService.getEarnings().subscribe((e) => {
      this.earnings.set(e);
      this.isLoading.set(false);
    });
    this.creatorService
      .getPosts({ page: 1, pageSize: 3 })
      .subscribe((r) => {
        if (r.success && r.data) {
          this.recentPosts.set(r.data);
        }
      });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadData();
    setTimeout(() => event.target.complete(), 1000);
  }

  getStatusColor(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'medium';
      case PostStatus.PendingReview: return 'warning';
      case PostStatus.Published: return 'success';
      case PostStatus.Rejected: return 'danger';
      default: return 'medium';
    }
  }

  getStatusLabel(status: PostStatus): string {
    switch (status) {
      case PostStatus.Draft: return 'Draft';
      case PostStatus.PendingReview: return 'Pending';
      case PostStatus.Published: return 'Published';
      case PostStatus.Rejected: return 'Rejected';
      default: return 'Unknown';
    }
  }
}
