import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpContext } from '@angular/common/http';
import { ApiService } from './api.service';
import { ApiResponse } from '../../models';
import { SUPPRESS_ERROR_TOAST } from '../interceptors/error.interceptor';
import {
  KibbeScoreRequest,
  KibbeScoreResponse,
  StyleProfileResponse,
  SaveStyleProfileRequest,
  OverrideStyleProfileRequest,
} from '../../models/style/kibbe.models';

@Injectable({ providedIn: 'root' })
export class KibbeQuizApiService {
  private readonly api = inject(ApiService);

  score(request: KibbeScoreRequest): Observable<ApiResponse<KibbeScoreResponse>> {
    return this.api.post<KibbeScoreResponse>('style', 'quiz/kibbe/score', request);
  }

  getProfile(): Observable<ApiResponse<StyleProfileResponse>> {
    const context = new HttpContext().set(SUPPRESS_ERROR_TOAST, true);
    return this.api.get<StyleProfileResponse>('style', 'profile', undefined, { context });
  }

  saveProfile(request: SaveStyleProfileRequest): Observable<ApiResponse<StyleProfileResponse>> {
    return this.api.put<StyleProfileResponse>('style', 'profile', request);
  }

  overrideProfile(request: OverrideStyleProfileRequest): Observable<ApiResponse<StyleProfileResponse>> {
    return this.api.put<StyleProfileResponse>('style', 'profile/override', request);
  }

  clearOverride(): Observable<ApiResponse<void>> {
    return this.api.delete<void>('style', 'profile/override');
  }
}
