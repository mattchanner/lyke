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
  IonSegment,
  IonSegmentButton,
  IonLabel,
  AlertController,
} from '@ionic/angular/standalone';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeQuizApiService } from '../../../../core/services/kibbe-quiz-api.service';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import { ToastService } from '../../../../core/services/toast.service';
import { ShareCardService } from '../../../../core/services/share-card.service';
import { KibbeScoreResponse, KibbeFamily, getMixedTypeName } from '../../../../models/style/kibbe.models';
import { FAMILY_TAGLINES } from '../teaser/style-quiz-teaser.page';
import {
  KIBBE_STYLE_GUIDES,
  KibbeStyleGuide,
} from '../../data/kibbe-style-guide.data';

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
    IonSegment,
    IonSegmentButton,
    IonLabel,
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
  readonly isSharing = signal(false);
  readonly activeTab = signal('overview');
  readonly displayFamily = signal<KibbeFamily | null>(null);
  readonly isOverriding = signal(false);
  readonly isOverride = signal(false);
  readonly sharePreviewUrl = signal<string | null>(null);

  tagline(family: string): string {
    return FAMILY_TAGLINES[family] ?? '';
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
      this.toast.info('Your session expired — please take the quiz again.');
      this.router.navigate(['/style-quiz']);
      return;
    }
    this.result.set(r);
    this.displayFamily.set(r.primaryFamily);
    this.analytics.fullResultsView(r.primaryFamily);
    this.saveProfile(r);
    this.generatePreview(r.primaryFamily);
  }

  setTab(tab: string): void {
    this.activeTab.set(tab);
  }

  private saveProfile(score: KibbeScoreResponse): void {
    this.isSaving.set(true);
    this.api.saveProfile({ score }).subscribe({
      next: (res) => {
        if (res.success) {
          this.analytics.styleProfileSaved(score.primaryFamily);
          this.session.clear();
        } else {
          this.toast.error('Could not save your style profile. You can retry from your profile page.');
        }
      },
      error: () =>
        this.toast.error('Could not save your style profile. You can retry from your profile page.'),
      complete: () => this.isSaving.set(false),
    });
  }

  private generatePreview(family: KibbeFamily): void {
    if (!isPlatformBrowser(this.platformId)) return;
    const tl = FAMILY_TAGLINES[family] ?? '';
    this.shareCard.generate(family, tl).then((blob) => {
      if (blob) {
        const url = URL.createObjectURL(blob);
        this.sharePreviewUrl.set(url);
      }
    });
  }

  async share(): Promise<void> {
    const family = this.displayFamily();
    if (!family || !isPlatformBrowser(this.platformId)) return;

    this.analytics.shareClick();
    this.isSharing.set(true);

    try {
      const blob = await this.shareCard.generate(family, FAMILY_TAGLINES[family] ?? '');
      if (!blob) throw new Error('Could not generate share card');

      const file = new File([blob], `lyke-style-${family.toLowerCase()}.png`, { type: 'image/png' });

      if (navigator.canShare && navigator.canShare({ files: [file] })) {
        await navigator.share({
          title: `My LYKE Style Type: ${family}`,
          text: `I'm a ${family} style type! Find yours at ${environment.appUrl}/style-quiz`,
          files: [file],
        });
      } else {
        // Download fallback
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = file.name;
        a.click();
        URL.revokeObjectURL(url);
      }

      this.analytics.shareComplete();
    } catch (err: unknown) {
      // User cancelled the share sheet — not an error
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
          this.generatePreview(family);
        } else {
          this.toast.error('Could not save your selection. Please try again.');
        }
      },
      error: () => this.toast.error('Could not save your selection. Please try again.'),
      complete: () => this.isOverriding.set(false),
    });
  }
}
