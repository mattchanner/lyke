import { Component } from '@angular/core';
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
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  imagesOutline,
  addCircleOutline,
  analyticsOutline,
  walletOutline,
  checkmarkCircleOutline,
} from 'ionicons/icons';

@Component({
  selector: 'app-creator-dashboard',
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
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/profile"></ion-back-button>
        </ion-buttons>
        <ion-title>Creator Dashboard</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <ion-item routerLink="/creator/posts" detail>
          <ion-icon name="images-outline" slot="start"></ion-icon>
          <ion-label>My Posts</ion-label>
        </ion-item>
        <ion-item routerLink="/creator/posts/create" detail>
          <ion-icon name="add-circle-outline" slot="start"></ion-icon>
          <ion-label>Create Post</ion-label>
        </ion-item>
        <ion-item routerLink="/creator/analytics" detail>
          <ion-icon name="analytics-outline" slot="start"></ion-icon>
          <ion-label>Analytics</ion-label>
        </ion-item>
        <ion-item routerLink="/creator/earnings" detail>
          <ion-icon name="wallet-outline" slot="start"></ion-icon>
          <ion-label>Earnings</ion-label>
        </ion-item>
        <ion-item routerLink="/creator/verification" detail>
          <ion-icon name="checkmark-circle-outline" slot="start"></ion-icon>
          <ion-label>Verification</ion-label>
        </ion-item>
      </ion-list>
    </ion-content>
  `,
})
export class DashboardPage {
  constructor() {
    addIcons({
      imagesOutline,
      addCircleOutline,
      analyticsOutline,
      walletOutline,
      checkmarkCircleOutline,
    });
  }
}
