import { Component, input, output } from '@angular/core';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonIcon,
  IonContent,
  IonList,
  IonItem,
  IonLabel,
  IonBadge,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { closeOutline } from 'ionicons/icons';
import { ImportProductsResponse } from '../../../models';

@Component({
  selector: 'app-import-results-modal',
  standalone: true,
  imports: [
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonContent,
    IonList,
    IonItem,
    IonLabel,
    IonBadge,
  ],
  templateUrl: './import-results-modal.component.html',
  styleUrls: ['./import-results-modal.component.scss'],
})
export class ImportResultsModalComponent {
  readonly isOpen = input(false);
  readonly result = input<ImportProductsResponse | null>(null);

  readonly dismissed = output<void>();

  constructor() {
    addIcons({ closeOutline });
  }

  close(): void {
    this.dismissed.emit();
  }
}
