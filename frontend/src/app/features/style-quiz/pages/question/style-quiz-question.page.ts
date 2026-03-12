import { Component, inject, OnInit, signal } from '@angular/core';
import { Router, ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonToolbar,
  IonButtons,
  IonBackButton,
  IonProgressBar,
} from '@ionic/angular/standalone';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import {
  KIBBE_QUIZ_SECTIONS,
  KIBBE_QUESTIONS_BY_ID,
} from '../../data/kibbe-quiz.data';
import { KibbeQuizQuestion, KibbeOption } from '../../../../models/style/kibbe.models';

@Component({
  selector: 'app-style-quiz-question',
  standalone: true,
  imports: [IonContent, IonHeader, IonToolbar, IonButtons, IonBackButton, IonProgressBar],
  templateUrl: './style-quiz-question.page.html',
  styleUrls: ['./style-quiz-question.page.scss'],
})
export class StyleQuizQuestionPage implements OnInit {
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly session = inject(KibbeSessionService);
  private readonly analytics = inject(KibbeAnalyticsService);

  private readonly allQuestions: KibbeQuizQuestion[] = ([] as KibbeQuizQuestion[]).concat(...KIBBE_QUIZ_SECTIONS.map((s) => s.questions));

  readonly question = signal<KibbeQuizQuestion | null>(null);
  readonly selectedOption = signal<KibbeOption | null>(null);
  readonly questionIndex = signal(0);
  readonly totalQuestions = signal(this.allQuestions.length);

  get progressValue(): number {
    return this.questionIndex() / this.totalQuestions();
  }

  ngOnInit(): void {
    const questionId = this.route.snapshot.paramMap.get('questionId') ?? '';
    const q = KIBBE_QUESTIONS_BY_ID[questionId] ?? null;
    this.question.set(q);

    if (q) {
      const saved = this.session.getAnswers()[questionId];
      if (saved) this.selectedOption.set(saved.selectedOption);
      this.questionIndex.set(this.allQuestions.findIndex((x) => x.id === questionId));
    }
  }

  select(option: KibbeOption): void {
    this.selectedOption.set(option);
    setTimeout(() => this.advance(), 400);
  }

  isSelected(option: KibbeOption): boolean {
    return this.selectedOption() === option;
  }

  advance(): void {
    const q = this.question();
    const opt = this.selectedOption();
    if (!q || !opt) return;

    this.session.setAnswer({ sectionId: q.sectionId, questionId: q.id, selectedOption: opt });

    const idx = this.allQuestions.findIndex((x) => x.id === q.id);
    const next = this.allQuestions[idx + 1];

    if (next) {
      if (next.sectionId !== q.sectionId) {
        this.analytics.sectionComplete(q.sectionId);
        this.router.navigate(['/style-quiz/section', next.sectionId]);
      } else {
        this.router.navigate(['/style-quiz/question', next.id]);
      }
    } else {
      this.analytics.sectionComplete(q.sectionId);
      this.analytics.quizComplete(String(this.allQuestions.length));
      this.router.navigate(['/style-quiz/calculating']);
    }
  }
}
