import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
} from '@ionic/angular/standalone';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/settings"></ion-back-button>
        </ion-buttons>
        <ion-title>Privacy</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content class="ion-padding">
      <h2>Your Privacy Matters</h2>
      <p>LYKE is designed with GDPR-first principles.</p>
      <ul>
        <li>Your body profile data is never shared directly with retailers</li>
        <li>All insights are aggregated and anonymized</li>
        <li>Profile displays show ranges, not exact measurements</li>
      </ul>
    </ion-content>
  `,
})
export class PrivacyPage {}
