import { Component, OnInit, signal, inject } from '@angular/core';
import { DecimalPipe } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonMenuButton,
  IonCard,
  IonCardHeader,
  IonCardTitle,
  IonCardContent,
  IonList,
  IonItem,
  IonLabel,
  IonIcon,
  IonSkeletonText,
  IonRefresher,
  IonRefresherContent,
  RefresherCustomEvent,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  storefrontOutline,
  cubeOutline,
  megaphoneOutline,
  analyticsOutline,
  bodyOutline,
  personOutline,
  personCircleOutline,
} from 'ionicons/icons';
import { RetailerService } from '../../../core';
import { RetailerProfileResponse } from '../../../models';

@Component({
  selector: 'app-retailer-dashboard',
  standalone: true,
  imports: [
    DecimalPipe,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonButtons,
    IonMenuButton,
    IonCard,
    IonCardHeader,
    IonCardTitle,
    IonCardContent,
    IonList,
    IonItem,
    IonLabel,
    IonIcon,
    IonSkeletonText,
    IonRefresher,
    IonRefresherContent,
  ],
  templateUrl: './retailer-dashboard.page.html',
  styleUrls: ['./retailer-dashboard.page.scss'],
})
export class RetailerDashboardPage implements OnInit {
  private readonly retailerService = inject(RetailerService);

  readonly profile = signal<RetailerProfileResponse | null>(null);
  readonly isLoading = signal(true);

  constructor() {
    addIcons({
      storefrontOutline,
      cubeOutline,
      megaphoneOutline,
      analyticsOutline,
      bodyOutline,
      personOutline,
      personCircleOutline,
    });
  }

  ngOnInit(): void {
    this.loadData();
  }

  loadData(refresh = false): void {
    if (!refresh) this.isLoading.set(true);
    this.retailerService.getProfile().subscribe((profile) => {
      this.profile.set(profile);
      this.isLoading.set(false);
    });
  }

  onRefresh(event: RefresherCustomEvent): void {
    this.loadData(true);
    setTimeout(() => event.target.complete(), 1000);
  }
}
