import { Component, inject, signal, computed } from '@angular/core';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonIcon,
  IonInput,
  IonItem,
  IonLabel,
  IonList,
  IonSpinner,
  AlertController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { warningOutline } from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core/services';

@Component({
  selector: 'app-delete-account',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonIcon,
    IonInput,
    IonItem,
    IonLabel,
    IonList,
    IonSpinner,
  ],
  templateUrl: './delete-account.page.html',
  styleUrls: ['./delete-account.page.scss'],
})
export class DeleteAccountPage {
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly alertController = inject(AlertController);

  readonly confirmationText = signal('');
  readonly isDeleting = signal(false);
  readonly isConfirmed = computed(() => this.confirmationText().toUpperCase() === 'DELETE');

  constructor() {
    addIcons({ warningOutline });
  }

  async onDelete(): Promise<void> {
    const alert = await this.alertController.create({
      header: 'Are you sure?',
      message: 'This will permanently delete your account and all associated data. This cannot be undone.',
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Delete',
          role: 'destructive',
          handler: () => this.performDeletion(),
        },
      ],
    });
    await alert.present();
  }

  private performDeletion(): void {
    this.isDeleting.set(true);
    this.authService.deleteAccount().subscribe({
      next: async () => {
        await this.toast.success('Your account has been deleted');
        await this.authService.logout();
      },
      error: async (err) => {
        this.isDeleting.set(false);
        await this.toast.error(err.message || 'Failed to delete account');
      },
    });
  }
}
