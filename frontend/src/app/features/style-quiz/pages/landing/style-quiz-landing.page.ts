import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  AlertController,
} from '@ionic/angular/standalone';
import { KibbeAnalyticsService } from '../../../../core/services/kibbe-analytics.service';
import { KibbeSessionService } from '../../../../core/services/kibbe-session.service';
import { KibbeQuizApiService } from '../../../../core/services/kibbe-quiz-api.service';
import { AuthService } from '../../../../core/services/auth.service';

@Component({
  selector: 'app-style-quiz-landing',
  standalone: true,
  imports: [IonContent, IonHeader, IonTitle, IonToolbar, IonButton],
  templateUrl: './style-quiz-landing.page.html',
  styleUrls: ['./style-quiz-landing.page.scss'],
})
export class StyleQuizLandingPage implements OnInit {
  private readonly router = inject(Router);
  private readonly analytics = inject(KibbeAnalyticsService);
  private readonly session = inject(KibbeSessionService);
  private readonly api = inject(KibbeQuizApiService);
  private readonly auth = inject(AuthService);
  private readonly alertCtrl = inject(AlertController);

  ngOnInit(): void {
    this.analytics.quizStart();
  }

  async start(): Promise<void> {
    const hasLocalSession = this.session.hasSession();

    if (hasLocalSession) {
      const confirmed = await this.showReplaceAlert();
      if (!confirmed) return;
      this.session.clear();
      this.router.navigate(['/style-quiz/primer']);
      return;
    }

    if (this.auth.isAuthenticated()) {
      this.api.getProfile().subscribe({
        next: async (res) => {
          if (res.success && res.data?.primaryFamily) {
            const confirmed = await this.showReplaceAlert();
            if (!confirmed) return;
          }
          this.router.navigate(['/style-quiz/primer']);
        },
        error: () => {
          // No profile or error — just proceed
          this.router.navigate(['/style-quiz/primer']);
        },
      });
    } else {
      this.router.navigate(['/style-quiz/primer']);
    }
  }

  private showReplaceAlert(): Promise<boolean> {
    return new Promise(async (resolve) => {
      const alert = await this.alertCtrl.create({
        header: 'Replace existing result?',
        message: 'You already have a result saved. Starting a new quiz will replace it. Continue?',
        buttons: [
          {
            text: 'Cancel',
            role: 'cancel',
            handler: () => resolve(false),
          },
          {
            text: 'Continue',
            handler: () => resolve(true),
          },
        ],
      });
      await alert.present();
    });
  }
}
