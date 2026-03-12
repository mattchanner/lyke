import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { IonContent, IonSpinner } from '@ionic/angular/standalone';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeQuizApiService } from '../../../../core/services/kibbe-quiz-api.service';
import { ToastService } from '../../../../core/services/toast.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-style-quiz-calculating',
  standalone: true,
  imports: [IonContent, IonSpinner],
  templateUrl: './style-quiz-calculating.page.html',
  styleUrls: ['./style-quiz-calculating.page.scss'],
})
export class StyleQuizCalculatingPage implements OnInit {
  private readonly router = inject(Router);
  private readonly session = inject(KibbeSessionService);
  private readonly api = inject(KibbeQuizApiService);
  private readonly toast = inject(ToastService);
  private readonly auth = inject(AuthService);

  ngOnInit(): void {
    const answers = Object.values(this.session.getAnswers());
    if (answers.length === 0) {
      this.router.navigate(['/style-quiz']);
      return;
    }

    const minDelay = new Promise<void>((r) => setTimeout(r, 2500));

    const scoreCall = new Promise<void>((resolve, reject) => {
      this.api.score({ answers }).subscribe({
        next: (res) => {
          if (res.success && res.data) {
            this.session.setScoreResult(res.data);
            resolve();
          } else {
            reject(new Error(res.error?.message ?? 'Scoring failed'));
          }
        },
        error: reject,
      });
    });

    Promise.all([minDelay, scoreCall])
      .then(() => this.router.navigate([this.auth.isAuthenticated() ? '/style-quiz/results' : '/style-quiz/teaser']))
      .catch(() => {
        this.toast.error('Something went wrong. Please try again.');
        this.router.navigate(['/style-quiz']);
      });
  }
}
