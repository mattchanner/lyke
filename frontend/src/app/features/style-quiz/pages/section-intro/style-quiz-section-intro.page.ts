import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonBackButton,
} from '@ionic/angular/standalone';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import { KIBBE_QUIZ_SECTIONS } from '../../data/kibbe-quiz.data';
import { KibbeQuizSection, KibbeSectionId } from '../../../../models/style/kibbe.models';

@Component({
  selector: 'app-style-quiz-section-intro',
  standalone: true,
  imports: [IonContent, IonHeader, IonTitle, IonToolbar, IonButton, IonButtons, IonBackButton],
  templateUrl: './style-quiz-section-intro.page.html',
  styleUrls: ['./style-quiz-section-intro.page.scss'],
})
export class StyleQuizSectionIntroPage implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly analytics = inject(KibbeAnalyticsService);

  readonly section = signal<KibbeQuizSection | null>(null);
  readonly sectionNumber = signal(1);

  ngOnInit(): void {
    const sectionId = this.route.snapshot.paramMap.get('sectionId') as KibbeSectionId;
    const found = KIBBE_QUIZ_SECTIONS.find((s) => s.id === sectionId) ?? null;
    this.section.set(found);
    this.sectionNumber.set(KIBBE_QUIZ_SECTIONS.findIndex((s) => s.id === sectionId) + 1);
    if (found) {
      this.analytics.sectionStart(sectionId);
    }
  }

  startSection(): void {
    const s = this.section();
    if (!s) return;
    this.router.navigate(['/style-quiz/question', s.questions[0].id]);
  }
}
