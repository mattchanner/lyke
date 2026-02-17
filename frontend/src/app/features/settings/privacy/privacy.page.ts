import { Component, inject, OnInit, signal } from '@angular/core';
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
  IonToggle,
  IonSpinner,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { shieldCheckmarkOutline, bodyOutline, statsChartOutline, resizeOutline, downloadOutline, trashOutline } from 'ionicons/icons';
import { ApiService, ToastService } from '../../../core/services';
import { ConsentStatusResponse, DataExportResponse } from '../../../models';

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
    IonToggle,
    IonSpinner,
  ],
  templateUrl: './privacy.page.html',
  styleUrls: ['./privacy.page.scss'],
})
export class PrivacyPage implements OnInit {
  private readonly api = inject(ApiService);
  private readonly toast = inject(ToastService);

  readonly isExporting = signal(false);
  readonly consentStatus = signal<ConsentStatusResponse | null>(null);

  constructor() {
    addIcons({ shieldCheckmarkOutline, bodyOutline, statsChartOutline, resizeOutline, downloadOutline, trashOutline });
  }

  ngOnInit(): void {
    this.api.get<ConsentStatusResponse>('privacy', 'consent').subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.consentStatus.set(res.data);
        }
      },
    });
  }

  async requestDataExport(): Promise<void> {
    this.isExporting.set(true);
    this.api.get<DataExportResponse>('privacy', 'export').subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const blob = new Blob([JSON.stringify(res.data, null, 2)], { type: 'application/json' });
          const url = URL.createObjectURL(blob);
          const a = document.createElement('a');
          a.href = url;
          a.download = `lyke-data-export-${new Date().toISOString().slice(0, 10)}.json`;
          a.click();
          URL.revokeObjectURL(url);
          this.toast.success('Data export downloaded');
        }
        this.isExporting.set(false);
      },
      error: () => {
        this.isExporting.set(false);
        this.toast.error('Failed to export data');
      },
    });
  }

  onMarketingToggle(event: CustomEvent): void {
    const value = event.detail.checked as boolean;
    this.api.put('privacy', 'consent', { marketingOptIn: value }).subscribe({
      next: () => {
        const current = this.consentStatus();
        if (current) {
          this.consentStatus.set({ ...current, marketingOptIn: value });
        }
      },
      error: () => {
        this.toast.error('Failed to update preference');
      },
    });
  }
}
