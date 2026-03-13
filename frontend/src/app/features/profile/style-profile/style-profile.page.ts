import { Component, inject, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../../environments/environment';
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
  IonSegment,
  IonSegmentButton,
  IonLabel,
  AlertController,
} from '@ionic/angular/standalone';
import { ViewWillEnter } from '@ionic/angular';
import { KibbeQuizApiService } from '../../../core/services/kibbe-quiz-api.service';
import { KibbeAnalyticsService } from '../../../core/services/kibbe-analytics.service';
import { ShareCardService } from '../../../core/services/share-card.service';
import { ToastService } from '../../../core/services/toast.service';
import { StyleProfileResponse, KibbeFamily, getMixedTypeName } from '../../../models/style/kibbe.models';
import { FAMILY_TAGLINES } from '../../style-quiz/pages/teaser/style-quiz-teaser.page';
import {
  KIBBE_STYLE_GUIDES,
  KibbeStyleGuide,
} from '../../style-quiz/data/kibbe-style-guide.data';

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
    IonSegment,
    IonSegmentButton,
    IonLabel,
  ],
  templateUrl: './style-profile.page.html',
  styleUrls: ['./style-profile.page.scss'],
})
export class StyleProfilePage implements ViewWillEnter {
  private readonly router = inject(Router);
  private readonly api = inject(KibbeQuizApiService);
  private readonly toast = inject(ToastService);
  private readonly alertCtrl = inject(AlertController);
  private readonly analytics = inject(KibbeAnalyticsService);
  private readonly shareCard = inject(ShareCardService);
  private readonly platformId = inject(PLATFORM_ID);

  readonly profile = signal<StyleProfileResponse | null>(null);
  readonly isLoading = signal(false);
  readonly isOverriding = signal(false);
  readonly isSharing = signal(false);
  readonly sharePreviewUrl = signal<string | null>(null);
  readonly activeTab = signal('overview');

  ionViewWillEnter(): void {
    this.loadProfile();
  }

  private loadProfile(): void {
    this.isLoading.set(true);
    this.api.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile.set(res.data);
          this.generatePreview();
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

  private generatePreview(): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const family = this.displayFamily();
    if (!family) return;
    const tl = FAMILY_TAGLINES[family] ?? '';
    this.shareCard.generate(family, tl).then((blob) => {
      if (blob) {
        const url = URL.createObjectURL(blob);
        this.sharePreviewUrl.set(url);
      }
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

  mixedTypeName(): string | null {
    const p = this.profile();
    if (!p?.isMixed) return null;
    return getMixedTypeName(p.primaryFamily!, p.runnerUpFamily);
  }

  styleGuide(family: string | null): KibbeStyleGuide | null {
    if (!family) return null;
    return KIBBE_STYLE_GUIDES[family] ?? null;
  }

  setTab(tab: string): void {
    this.activeTab.set(tab);
  }

  async share(): Promise<void> {
    const family = this.displayFamily();
    if (!family || !isPlatformBrowser(this.platformId)) return;

    this.analytics.shareClick();
    this.isSharing.set(true);

    try {
      const blob = await this.shareCard.generate(family, FAMILY_TAGLINES[family] ?? '');
      if (!blob) throw new Error('Could not generate share card');

      const file = new File([blob], `lyke-style-${family.toLowerCase()}.png`, {
        type: 'image/png',
      });

      if (navigator.canShare && navigator.canShare({ files: [file] })) {
        await navigator.share({
          title: `My LYKE Style Type: ${family}`,
          text: `I'm a ${family} style type! Find yours at ${environment.appUrl}/style-quiz`,
          files: [file],
        });
      } else {
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = file.name;
        a.click();
        URL.revokeObjectURL(url);
      }

      this.analytics.shareComplete();
    } catch (err: unknown) {
      if (err instanceof Error && err.name !== 'AbortError') {
        this.toast.error('Could not share. Try downloading instead.');
      }
    } finally {
      this.isSharing.set(false);
    }
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
          this.generatePreview();
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
