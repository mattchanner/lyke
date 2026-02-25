import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';
import { ApiResponse } from '../../models/api/api-response.model';
import { QuizAnswer, QuizResponse } from '../../models/quiz/quiz.model';

@Injectable({ providedIn: 'root' })
export class QuizService {
  private readonly api = inject(ApiService);

  calculate(answers: QuizAnswer[]): Observable<ApiResponse<QuizResponse>> {
    return this.api.post<QuizResponse>('quiz', 'calculate', { answers });
  }
}
