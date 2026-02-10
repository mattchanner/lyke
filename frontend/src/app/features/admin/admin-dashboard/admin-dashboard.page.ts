import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonList,
  IonItem,
  IonLabel,
  IonIcon,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonBadge,
  IonSpinner,
  IonRefresher,
  IonRefresherContent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  imagesOutline,
  peopleOutline,
  checkmarkCircleOutline,
  trendingUpOutline,
  personOutline,
  documentTextOutline,
  eyeOutline,
  heartOutline,
  chevronForwardOutline,
} from 'ionicons/icons';

import { AdminService } from '../../../core/services';
import { ToastService } from '../../../core/services';
import { PlatformStatsResponse } from '../../../models';

@Component({
  selector: 'app-admin-dashboard',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonList,
    IonItem,
    IonLabel,
    IonIcon,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonBadge,
    IonSpinner,
    IonRefresher,
    IonRefresherContent,
  ],
  templateUrl: './admin-dashboard.page.html',
  styleUrls: ['./admin-dashboard.page.scss'],
})
export class AdminDashboardPage implements OnInit {
  private readonly adminService = inject(AdminService);
  private readonly toast = inject(ToastService);

  readonly stats = signal<PlatformStatsResponse | null>(null);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({
      imagesOutline,
      peopleOutline,
      checkmarkCircleOutline,
      trendingUpOutline,
      personOutline,
      documentTextOutline,
      eyeOutline,
      heartOutline,
      chevronForwardOutline,
    });
  }

  ngOnInit(): void {
    this.loadStats();
  }

  loadStats(): void {
    this.isLoading.set(true);
    this.adminService.getPlatformStats().subscribe({
      next: (stats) => this.stats.set(stats),
      error: () => this.toast.error('Failed to load stats'),
      complete: () => this.isLoading.set(false),
    });
  }

  onRefresh(event: CustomEvent): void {
    this.loadStats();
    setTimeout(() => {
      (event.target as HTMLIonRefresherElement).complete();
    }, 500);
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
