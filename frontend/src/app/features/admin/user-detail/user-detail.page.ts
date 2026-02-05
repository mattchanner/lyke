import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons } from '@ionic/angular/standalone';

@Component({
  selector: 'app-user-detail',
  standalone: true,
  imports: [CommonModule, IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start"><ion-back-button defaultHref="/admin/users"></ion-back-button></ion-buttons>
        <ion-title>User Detail</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content class="ion-padding"><p>User details coming soon...</p></ion-content>
  `,
})
export class UserDetailPage {}
