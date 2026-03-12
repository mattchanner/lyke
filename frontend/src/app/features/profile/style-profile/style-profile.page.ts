import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonBackButton,
  IonSpinner,
  AlertController,
} from '@ionic/angular/standalone';
import { ViewWillEnter } from '@ionic/angular';
import { KibbeQuizApiService } from '../../../core/services/kibbe-quiz-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { StyleProfileResponse, KibbeFamily } from '../../../models/style/kibbe.models';
import { FAMILY_TAGLINES } from '../../style-quiz/pages/teaser/style-quiz-teaser.page';

@Component({
  selector: 'app-style-profile',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonButtons,
    IonBackButton,
    IonSpinner,
  ],
  templateUrl: './style-profile.page.html',
  styleUrls: ['./style-profile.page.scss'],
})
export class StyleProfilePage implements ViewWillEnter {
  private readonly router = inject(Router);
  private readonly api = inject(KibbeQuizApiService);
  private readonly toast = inject(ToastService);
  private readonly alertCtrl = inject(AlertController);

  readonly profile = signal<StyleProfileResponse | null>(null);
  readonly isLoading = signal(false);
  readonly isOverriding = signal(false);

  ionViewWillEnter(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.isLoading.set(true);
    this.api.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile.set(res.data);
        } else {
          this.profile.set(null);
        }
      },
      error: (err) => {
        this.profile.set(null);
        this.isLoading.set(false);
        if (err?.status !== 404) {
          this.toast.error('Could not load your style profile.');
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  displayFamily(): KibbeFamily | null {
    const p = this.profile();
    if (!p) return null;
    return p.isUserOverride ? p.overrideFamily : p.primaryFamily;
  }

  tagline(family: string | null): string {
    if (!family) return '';
    return FAMILY_TAGLINES[family] ?? '';
  }

  retakeQuiz(): void {
    this.router.navigate(['/style-quiz']);
  }

  async changeFamily(): Promise<void> {
    const alert = await this.alertCtrl.create({
      header: 'Choose a different family',
      inputs: [
        { type: 'radio', label: 'Dramatic', value: 'Dramatic' },
        { type: 'radio', label: 'Natural', value: 'Natural' },
        { type: 'radio', label: 'Classic', value: 'Classic' },
        { type: 'radio', label: 'Gamine', value: 'Gamine' },
        { type: 'radio', label: 'Romantic', value: 'Romantic' },
      ],
      buttons: [
        { text: 'Cancel', role: 'cancel' },
        {
          text: 'Select',
          handler: (selectedFamily: KibbeFamily) => {
            if (selectedFamily) {
              this.applyOverride(selectedFamily);
            }
          },
        },
      ],
    });
    await alert.present();
  }

  private applyOverride(family: KibbeFamily): void {
    this.isOverriding.set(true);
    this.api.overrideProfile({ family }).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile.set(res.data);
        } else {
          this.toast.error('Could not save your selection. Please try again.');
        }
      },
      error: () => this.toast.error('Could not save your selection. Please try again.'),
      complete: () => this.isOverriding.set(false),
    });
  }

  clearOverride(): void {
    this.isOverriding.set(true);
    this.api.clearOverride().subscribe({
      next: (res) => {
        if (res.success) {
          this.loadProfile();
        } else {
          this.toast.error('Could not clear override. Please try again.');
          this.isOverriding.set(false);
        }
      },
      error: () => {
        this.toast.error('Could not clear override. Please try again.');
        this.isOverriding.set(false);
      },
    });
  }
}
