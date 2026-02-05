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
import { imagesOutline, peopleOutline, checkmarkCircleOutline } from 'ionicons/icons';

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
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start"><ion-back-button defaultHref="/profile"></ion-back-button></ion-buttons>
        <ion-title>Admin</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <ion-list>
        <ion-item routerLink="/admin/posts" detail>
          <ion-icon name="images-outline" slot="start"></ion-icon>
          <ion-label>Post Moderation</ion-label>
        </ion-item>
        <ion-item routerLink="/admin/users" detail>
          <ion-icon name="people-outline" slot="start"></ion-icon>
          <ion-label>User Management</ion-label>
        </ion-item>
        <ion-item routerLink="/admin/verifications" detail>
          <ion-icon name="checkmark-circle-outline" slot="start"></ion-icon>
          <ion-label>Verification Reviews</ion-label>
        </ion-item>
      </ion-list>
    </ion-content>
  `,
})
export class AdminDashboardPage {
  constructor() {
    addIcons({ imagesOutline, peopleOutline, checkmarkCircleOutline });
  }
}
