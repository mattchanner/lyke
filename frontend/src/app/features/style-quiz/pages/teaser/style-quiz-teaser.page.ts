import { Component, inject, OnInit, signal } from '@angular/core';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
} from '@ionic/angular/standalone';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import { KibbeScoreResponse } from '../../../../models/style/kibbe.models';

export const FAMILY_TAGLINES: Record<string, string> = {
  Dramatic: 'Bold, elongated, and powerfully angular.',
  Natural: 'Relaxed, earthy, and effortlessly at ease.',
  Classic: 'Refined, symmetrical, and timelessly elegant.',
  Gamine: 'High contrast and compact — angular structure with a playful, rounded spark.',
  Romantic: 'Soft, lush, and sensuously feminine.',
};

@Component({
  selector: 'app-style-quiz-teaser',
  standalone: true,
  imports: [IonContent, IonHeader, IonTitle, IonToolbar, IonButton],
  templateUrl: './style-quiz-teaser.page.html',
  styleUrls: ['./style-quiz-teaser.page.scss'],
})
export class StyleQuizTeaserPage implements OnInit {
  private readonly router = inject(Router);
  private readonly session = inject(KibbeSessionService);
  private readonly analytics = inject(KibbeAnalyticsService);

  readonly result = signal<KibbeScoreResponse | null>(null);
  readonly tagline = signal('');

  ngOnInit(): void {
    const r = this.session.getScoreResult();
    this.result.set(r);
    if (r) {
      this.tagline.set(FAMILY_TAGLINES[r.primaryFamily] ?? '');
      this.analytics.teaserView(r.primaryFamily);
    }
  }

  register(): void {
    this.analytics.teaserRegisterClick();
    this.router.navigate(['/auth/register'], {
      queryParams: { return: 'style-quiz/results' },
    });
  }

  login(): void {
    this.analytics.teaserRegisterClick();
    this.router.navigate(['/auth/login'], {
      queryParams: { return: 'style-quiz/results' },
    });
  }

  retake(): void {
    this.session.clear();
    this.router.navigate(['/style-quiz']);
  }
}
