import { Component, inject } from '@angular/core';
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
  shieldOutline,
  trashOutline,
  informationCircleOutline,
  documentTextOutline,
} from 'ionicons/icons';
import { AuthService } from '../../../core';

@Component({
  selector: 'app-settings',
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
        <ion-title>Settings</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content>
      <ion-list>
        <ion-item routerLink="/settings/privacy" detail>
          <ion-icon name="shield-outline" slot="start"></ion-icon>
          <ion-label>Privacy</ion-label>
        </ion-item>

        <ion-item button>
          <ion-icon name="document-text-outline" slot="start"></ion-icon>
          <ion-label>Terms of Service</ion-label>
        </ion-item>

        <ion-item button>
          <ion-icon name="information-circle-outline" slot="start"></ion-icon>
          <ion-label>About</ion-label>
        </ion-item>

        <ion-item routerLink="/settings/delete-account" detail>
          <ion-icon name="trash-outline" slot="start" color="danger"></ion-icon>
          <ion-label color="danger">Delete Account</ion-label>
        </ion-item>
      </ion-list>
    </ion-content>
  `,
})
export class SettingsPage {
  constructor() {
    addIcons({
      shieldOutline,
      trashOutline,
      informationCircleOutline,
      documentTextOutline,
    });
  }
}
