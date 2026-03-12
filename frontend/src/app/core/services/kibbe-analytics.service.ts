import { Injectable, inject } from '@angular/core';
import { AnalyticsService } from './analytics.service';
import { KibbeFamily, KibbeSectionId } from '../../models/style/kibbe.models';

export type KibbeAnalyticsEvent =
  | 'quiz_start'
  | 'primer_view'
  | 'primer_continue'
  | 'section_start'
  | 'section_complete'
  | 'quiz_complete'
  | 'teaser_view'
  | 'teaser_register_click'
  | 'register_complete_from_quiz'
  | 'login_complete_from_quiz'
  | 'full_results_view'
  | 'style_profile_saved'
  | 'share_click'
  | 'share_complete';

@Injectable({ providedIn: 'root' })
export class KibbeAnalyticsService {
  private readonly analytics = inject(AnalyticsService);

  quizStart(): void {
    this.track('quiz_start');
  }

  primerView(): void {
    this.track('primer_view');
  }

  primerContinue(): void {
    this.track('primer_continue');
  }

  sectionStart(sectionId: KibbeSectionId): void {
    this.track('section_start', { section_id: sectionId });
  }

  sectionComplete(sectionId: KibbeSectionId): void {
    this.track('section_complete', { section_id: sectionId });
  }

  quizComplete(questionCount: string): void {
    this.track('quiz_complete', { question_count: questionCount });
  }

  teaserView(primaryFamily: KibbeFamily): void {
    this.track('teaser_view', { primary_family: primaryFamily });
  }

  teaserRegisterClick(): void {
    this.track('teaser_register_click');
  }

  registerCompleteFromQuiz(): void {
    this.track('register_complete_from_quiz');
  }

  loginCompleteFromQuiz(): void {
    this.track('login_complete_from_quiz');
  }

  fullResultsView(primaryFamily: KibbeFamily): void {
    this.track('full_results_view', { primary_family: primaryFamily });
  }

  styleProfileSaved(primaryFamily: KibbeFamily): void {
    this.track('style_profile_saved', { primary_family: primaryFamily });
  }

  shareClick(): void {
    this.track('share_click');
  }

  shareComplete(): void {
    this.track('share_complete');
  }

  private track(
    event: KibbeAnalyticsEvent,
    extra?: Record<string, string>
  ): void {
    this.analytics.track(`kibbe_${event}`, {
      feature: 'kibbe_quiz',
      ...extra,
    });
  }
}
