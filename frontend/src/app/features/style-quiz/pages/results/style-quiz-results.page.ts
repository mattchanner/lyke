import { Component, inject, OnInit, signal, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { environment } from '../../../../../environments/environment';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonSpinner,
  AlertController,
} from '@ionic/angular/standalone';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeQuizApiService } from '../../../../core/services/kibbe-quiz-api.service';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import { ToastService } from '../../../../core/services/toast.service';
import { ShareCardService } from '../../../../core/services/share-card.service';
import {
  KibbeScoreResponse,
  KibbeFamily,
  getMixedTypeName,
} from '../../../../models/style/kibbe.models';
import { FAMILY_TAGLINES } from '../teaser/style-quiz-teaser.page';
import {
  KIBBE_STYLE_GUIDES,
  KibbeStyleGuide,
} from '../../data/kibbe-style-guide.data';

const FAMILY_GLOSSES: Record<KibbeFamily, string> = {
  Dramatic: 'sharp, elongated lines',
  Natural: 'broad, relaxed lines',
  Classic: 'balanced, symmetrical proportions',
  Gamine: 'compact, sharp contrast',
  Romantic: 'soft, rounded curves',
};

@Component({
  selector: 'app-style-quiz-results',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonButtons,
    IonSpinner,
  ],
  templateUrl: './style-quiz-results.page.html',
  styleUrls: ['./style-quiz-results.page.scss'],
})
export class StyleQuizResultsPage implements OnInit {
  private readonly router = inject(Router);
  private readonly session = inject(KibbeSessionService);
  private readonly api = inject(KibbeQuizApiService);
  private readonly analytics = inject(KibbeAnalyticsService);
  private readonly toast = inject(ToastService);
  private readonly alertCtrl = inject(AlertController);
  private readonly shareCard = inject(ShareCardService);
  private readonly platformId = inject(PLATFORM_ID);

  readonly result = signal<KibbeScoreResponse | null>(null);
  readonly isSaving = signal(false);
  readonly saveError = signal(false);
  readonly isSharing = signal(false);
  readonly displayFamily = signal<KibbeFamily | null>(null);
  readonly isOverriding = signal(false);
  readonly isOverride = signal(false);

  tagline(family: string): string {
    return FAMILY_TAGLINES[family] ?? '';
  }

  familyGloss(family: KibbeFamily): string {
    return FAMILY_GLOSSES[family] ?? '';
  }

  mixedTypeName(): string | null {
    const r = this.result();
    if (!r?.isMixed) return null;
    return getMixedTypeName(r.primaryFamily, r.runnerUpFamily);
  }

  styleGuide(family: string): KibbeStyleGuide | null {
    return KIBBE_STYLE_GUIDES[family] ?? null;
  }

  ngOnInit(): void {
    const r = this.session.getScoreResult();
    if (!r) {
      this.toast.info('Your session expired. Please take the quiz again.');
      this.router.navigate(['/style-quiz']);
      return;
    }
    this.result.set(r);
    this.displayFamily.set(r.primaryFamily);
    this.analytics.fullResultsView(r.primaryFamily);
    this.saveProfile(r);
  }

  private saveProfile(score: KibbeScoreResponse): void {
    this.saveError.set(false);
    this.isSaving.set(true);
    this.api.saveProfile({ score }).subscribe({
      next: (res) => {
        if (res.success) {
          this.analytics.styleProfileSaved(score.primaryFamily);
          this.session.clear();
        } else {
          this.saveError.set(true);
        }
      },
      error: () => this.saveError.set(true),
      complete: () => this.isSaving.set(false),
    });
  }

  retrySave(): void {
    const r = this.result();
    if (r) this.saveProfile(r);
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

  done(): void {
    this.router.navigate(['/feed']);
  }

  retake(): void {
    this.session.clear();
    this.router.navigate(['/style-quiz']);
  }

  async chooseFamily(): Promise<void> {
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
        if (res.success) {
          this.displayFamily.set(family);
          this.isOverride.set(true);
        } else {
          this.toast.error('Could not save your selection. Please try again.');
        }
      },
      error: () => this.toast.error('Could not save your selection. Please try again.'),
      complete: () => this.isOverriding.set(false),
    });
  }
}
