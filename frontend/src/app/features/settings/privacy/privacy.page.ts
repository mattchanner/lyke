import { Component, inject } from '@angular/core';
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
  IonIcon,
  IonLabel,
  IonNote,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { shieldCheckmarkOutline, bodyOutline, statsChartOutline, resizeOutline, downloadOutline, trashOutline } from 'ionicons/icons';
import { ToastService } from '../../../core/services';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonList,
    IonItem,
    IonIcon,
    IonLabel,
    IonNote,
  ],
  templateUrl: './privacy.page.html',
  styleUrls: ['./privacy.page.scss'],
})
export class PrivacyPage {
  private readonly toast = inject(ToastService);

  constructor() {
    addIcons({ shieldCheckmarkOutline, bodyOutline, statsChartOutline, resizeOutline, downloadOutline, trashOutline });
  }

  async requestDataExport(): Promise<void> {
    await this.toast.info('Data export is coming soon');
  }
}
