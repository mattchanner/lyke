import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons } from '@ionic/angular/standalone';

@Component({
  selector: 'app-post-edit',
  standalone: true,
  imports: [CommonModule, IonContent, IonHeader, IonTitle, IonToolbar, IonBackButton, IonButtons],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start"><ion-back-button defaultHref="/creator/posts"></ion-back-button></ion-buttons>
        <ion-title>Edit Post</ion-title>
      </ion-toolbar>
    </ion-header>
    <ion-content class="ion-padding"><p>Post editor coming soon...</p></ion-content>
  `,
})
export class PostEditPage {}
