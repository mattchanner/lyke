import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonAvatar,
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
  IonRefresher,
  IonRefresherContent,
  IonSkeletonText,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  imagesOutline,
  peopleOutline,
  checkmarkCircleOutline,
  trendingUpOutline,
  personOutline,
  personCircleOutline,
  documentTextOutline,
  eyeOutline,
  heartOutline,
  chevronForwardOutline,
  refreshOutline,
  flagOutline,
  warningOutline,
} from 'ionicons/icons';

import { AdminService, AuthService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { PlatformStatsResponse } from '../../../models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonAvatar,
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
    IonRefresher,
    IonRefresherContent,
    IonSkeletonText,
  ],
  templateUrl: './admin-dashboard.page.html',
  styleUrls: ['./admin-dashboard.page.scss'],
})
export class AdminDashboardPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);
  readonly auth = inject(AuthService);

  readonly stats = signal<PlatformStatsResponse | null>(null);
  readonly isLoading = signal(false);
  readonly hasError = signal(false);

  constructor() {
    addIcons({
      imagesOutline,
      peopleOutline,
      checkmarkCircleOutline,
      trendingUpOutline,
      personOutline,
      personCircleOutline,
      documentTextOutline,
      eyeOutline,
      heartOutline,
      chevronForwardOutline,
      refreshOutline,
      flagOutline,
      warningOutline,
    });
  }

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(event?: CustomEvent): void {
    this.isLoading.set(true);
    this.hasError.set(false);
    this.adminService.getPlatformStats().subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => {
        this.toast.error('Failed to load stats');
        this.hasError.set(true);
        this.isLoading.set(false);
        (event?.target as any)?.complete();
      },
      complete: () => {
        this.isLoading.set(false);
        (event?.target as any)?.complete();
      },
    });
  }

  onRefresh(event: CustomEvent): void {
    this.loadStats(event);
  }

  formatNumber(num: number): string {
    if (num >= 1000000) {
      return (num / 1000000).toFixed(1) + 'M';
    }
    if (num >= 1000) {
      return (num / 1000).toFixed(1) + 'K';
    }
    return num.toString();
  }
}
