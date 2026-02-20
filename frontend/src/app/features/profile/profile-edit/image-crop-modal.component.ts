import { Component, Input, signal } from '@angular/core';
import {
  IonHeader,
  IonToolbar,
  IonTitle,
  IonContent,
  IonButtons,
  IonButton,
  IonSpinner,
  ModalController,
} from '@ionic/angular/standalone';
import {
  ImageCropperComponent,
  ImageCroppedEvent,
} from 'ngx-image-cropper';

@Component({
  selector: 'app-image-crop-modal',
  standalone: true,
  imports: [
    IonHeader,
    IonToolbar,
    IonTitle,
    IonContent,
    IonButtons,
    IonButton,
    IonSpinner,
    ImageCropperComponent,
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-button (click)="dismiss()">Cancel</ion-button>
        </ion-buttons>
        <ion-title>Crop Photo</ion-title>
        <ion-buttons slot="end">
          <ion-button
            (click)="confirm()"
            [disabled]="!croppedBlob()"
            [strong]="true"
          >
            @if (isCropping()) {
              <ion-spinner name="crescent"></ion-spinner>
            } @else {
              Done
            }
          </ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>
    <ion-content>
      <div class="cropper-wrapper">
        <image-cropper
          [imageFile]="imageFile"
          [maintainAspectRatio]="true"
          [aspectRatio]="1"
          [roundCropper]="true"
          [resizeToWidth]="512"
          format="jpeg"
          output="blob"
          [imageQuality]="85"
          (imageCropped)="onCropped($event)"
          (loadImageFailed)="onLoadFailed()"
        ></image-cropper>
      </div>
    </ion-content>
  `,
  styles: [`
    .cropper-wrapper {
      height: 100%;
      display: flex;
      align-items: center;
      justify-content: center;
      background: var(--ion-color-dark);
    }
  `],
})
export class ImageCropModalComponent {
  @Input() imageFile!: File;

  readonly croppedBlob = signal<Blob | null>(null);
  readonly isCropping = signal(false);

  constructor(private modalCtrl: ModalController) {}

  onCropped(event: ImageCroppedEvent): void {
    this.croppedBlob.set(event.blob ?? null);
  }

  onLoadFailed(): void {
    this.modalCtrl.dismiss(null);
  }

  dismiss(): void {
    this.modalCtrl.dismiss(null);
  }

  confirm(): void {
    this.modalCtrl.dismiss(this.croppedBlob());
  }
}
