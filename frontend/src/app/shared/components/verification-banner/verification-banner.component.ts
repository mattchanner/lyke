import { Component, inject, signal } from '@angular/core';
import {
  IonButton,
  IonIcon,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { mailOutline, closeOutline } from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core';

@Component({
  selector: 'app-verification-banner',
  standalone: true,
  imports: [IonButton, IonIcon],
  templateUrl: './verification-banner.component.html',
  styleUrls: ['./verification-banner.component.scss'],
})
export class VerificationBannerComponent {
  readonly auth = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly dismissed = signal(false);
  readonly cooldown = signal(false);

  constructor() {
    addIcons({ mailOutline, closeOutline });
  }

  dismiss(): void {
    this.dismissed.set(true);
  }

  resend(): void {
    if (this.cooldown()) return;

    this.cooldown.set(true);
    this.auth.resendVerification().subscribe({
      next: () => {
        this.toast.success('Verification email sent! Check your inbox.');
      },
      error: () => {
        this.toast.error('Failed to send verification email. Please try again.');
        this.cooldown.set(false);
      },
    });

    // Reset cooldown after 60 seconds
    setTimeout(() => this.cooldown.set(false), 60000);
  }
}
