import { Component, inject, signal, Input, OnInit } from '@angular/core';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonButtons,
  IonIcon,
  IonSpinner,
  IonProgressBar,
  ModalController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { chevronBackOutline, shareOutline, checkmarkCircleOutline } from 'ionicons/icons';
import { QuizService } from '../../core/services/quiz.service';
import { ToastService } from '../../core/services/toast.service';
import { QuizAnswer, QuizResponse } from '../../models/quiz/quiz.model';

interface QuizQuestion {
  id: number;
  prompt: string;
  answers: string[];
}

const QUIZ_QUESTIONS: QuizQuestion[] = [
  {
    id: 1,
    prompt: 'When you look in the mirror, how do your shoulders compare to your hips?',
    answers: [
      'My shoulders are noticeably wider than my hips',
      'My shoulders and hips are about the same width',
      'My hips are noticeably wider than my shoulders',
    ],
  },
  {
    id: 2,
    prompt: 'How would you describe your natural waistline?',
    answers: [
      'Very defined — my waist is noticeably narrower than my bust and hips',
      'Moderately defined — there\'s some curve but not dramatic',
      'Minimal definition — my waist, bust, and hips are similar measurements',
    ],
  },
  {
    id: 3,
    prompt: 'When you gain weight, where does it tend to go first?',
    answers: [
      'Upper body (shoulders, arms, bust area)',
      'Midsection (stomach, waist, back)',
      'Lower body (hips, thighs, bottom)',
      'Evenly distributed throughout my body',
    ],
  },
  {
    id: 4,
    prompt: 'When shopping for clothes, where do you most often need adjustments?',
    answers: [
      'Shoulders/bust area is often too tight',
      'Waist/midsection needs more room',
      'Hips/thighs are snug while waist gaps',
      'Clothes fit similarly throughout — alterations are consistent',
    ],
  },
  {
    id: 5,
    prompt: 'How would you describe your height?',
    answers: [
      'Petite (under 5\'3" / 160cm)',
      'Average (5\'3" - 5\'7" / 160-170cm)',
      'Tall (over 5\'7" / 170cm)',
    ],
  },
  {
    id: 6,
    prompt: 'Which best describes your typical clothing size range?',
    answers: [
      'Standard sizes (XS-XL, 0-14)',
      'Plus sizes (1X+, 16+)',
    ],
  },
];

@Component({
  selector: 'app-quiz',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonButtons,
    IonIcon,
    IonSpinner,
    IonProgressBar,
  ],
  templateUrl: './quiz.page.html',
  styleUrls: ['./quiz.page.scss'],
})
export class QuizPage implements OnInit {
  @Input() isEmbedded = false;

  private readonly quizService = inject(QuizService);
  private readonly toast = inject(ToastService);
  private readonly modalCtrl = inject(ModalController);

  readonly questions = QUIZ_QUESTIONS;
  readonly currentScreen = signal(0); // 0=intro, 1-6=questions, 7=results
  readonly selectedAnswers = signal<Map<number, number>>(new Map());
  readonly isLoading = signal(false);
  readonly result = signal<QuizResponse | null>(null);

  constructor() {
    addIcons({ chevronBackOutline, shareOutline, checkmarkCircleOutline });
  }

  ngOnInit(): void {}

  get currentQuestion(): QuizQuestion | null {
    const screen = this.currentScreen();
    if (screen >= 1 && screen <= 6) {
      return this.questions[screen - 1];
    }
    return null;
  }

  get progressValue(): number {
    const screen = this.currentScreen();
    if (screen < 1 || screen > 6) return 0;
    return screen / 6;
  }

  get currentSelectedAnswer(): number | null {
    const question = this.currentQuestion;
    if (!question) return null;
    return this.selectedAnswers().get(question.id) ?? null;
  }

  get canAdvance(): boolean {
    return this.currentSelectedAnswer !== null;
  }

  isAnswerSelected(index: number): boolean {
    return this.currentSelectedAnswer === index;
  }

  selectAnswer(index: number): void {
    const question = this.currentQuestion;
    if (!question) return;
    this.selectedAnswers.update((map) => {
      const next = new Map(map);
      next.set(question.id, index);
      return next;
    });
  }

  bodyShapeIcon(name: string): string {
    const slug = name.toLowerCase().replace(/\s+/g, '-');
    return `assets/body-shapes/${slug}.svg`;
  }

  start(): void {
    this.currentScreen.set(1);
  }

  back(): void {
    const screen = this.currentScreen();
    if (screen === 0) {
      this.modalCtrl.dismiss(null, 'cancel');
    } else {
      this.currentScreen.update((s) => s - 1);
    }
  }

  next(): void {
    const screen = this.currentScreen();
    if (screen === 6) {
      this.submitQuiz();
    } else if (screen < 6) {
      this.currentScreen.update((s) => s + 1);
    }
  }

  private submitQuiz(): void {
    this.isLoading.set(true);

    const answers: QuizAnswer[] = [];
    this.selectedAnswers().forEach((answerIndex, questionId) => {
      answers.push({ questionId, answerIndex });
    });

    this.quizService.calculate(answers).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.result.set(response.data);
          this.currentScreen.set(7);
        } else {
          this.toast.error(response.error?.message || 'Something went wrong. Please try again.');
        }
      },
      error: () => {
        this.toast.error('Something went wrong. Please try again.');
        this.isLoading.set(false);
      },
      complete: () => this.isLoading.set(false),
    });
  }

  applyResult(): void {
    this.modalCtrl.dismiss(this.result(), 'apply');
  }

  dismiss(): void {
    this.modalCtrl.dismiss(null, 'cancel');
  }

  shareResult(): void {
    // Future: generate shareable card
    this.toast.info('Sharing coming soon!');
  }

  joinLyke(): void {
    // Deep-link / navigate to register
    window.location.href = '/auth/register';
  }
}
