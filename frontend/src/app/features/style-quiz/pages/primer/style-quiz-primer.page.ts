import { Component, inject, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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

@Component({
  selector: 'app-style-quiz-primer',
  standalone: true,
  imports: [IonContent, IonHeader, IonTitle, IonToolbar, IonButton, IonButtons, IonBackButton],
  templateUrl: './style-quiz-primer.page.html',
  styleUrls: ['./style-quiz-primer.page.scss'],
})
export class StyleQuizPrimerPage implements OnInit {
  private readonly router = inject(Router);
  private readonly analytics = inject(KibbeAnalyticsService);

  ngOnInit(): void {
    this.analytics.primerView();
  }

  begin(): void {
    this.analytics.primerContinue();
    this.router.navigate(['/style-quiz/section', KIBBE_QUIZ_SECTIONS[0].id]);
  }
}
