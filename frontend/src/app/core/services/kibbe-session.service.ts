import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import {
  KibbeAnswer,
  KibbeScoreResponse,
} from '../../models/style/kibbe.models';

const SESSION_KEY = 'lyke_kibbe_session';
const SESSION_TTL_MS = 30 * 60 * 1000; // 30 minutes

interface KibbeSession {
  answers: Record<string, KibbeAnswer>;
  scoreResult: KibbeScoreResponse | null;
  startedAt: number;
}

@Injectable({ providedIn: 'root' })
export class KibbeSessionService {
  private readonly platformId = inject(PLATFORM_ID);

  private get isBrowser(): boolean {
    return isPlatformBrowser(this.platformId);
  }

  private load(): KibbeSession | null {
    if (!this.isBrowser) return null;
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return null;
    try {
      const session: KibbeSession = JSON.parse(raw);
      if (Date.now() - session.startedAt > SESSION_TTL_MS) {
        this.clear();
        return null;
      }
      return session;
    } catch {
      this.clear();
      return null;
    }
  }

  private save(session: KibbeSession): void {
    if (!this.isBrowser) return;
    localStorage.setItem(SESSION_KEY, JSON.stringify(session));
  }

  getAnswers(): Record<string, KibbeAnswer> {
    return this.load()?.answers ?? {};
  }

  setAnswer(answer: KibbeAnswer): void {
    const session = this.load() ?? {
      answers: {},
      scoreResult: null,
      startedAt: Date.now(),
    };
    session.answers[answer.questionId] = answer;
    this.save(session);
  }

  getScoreResult(): KibbeScoreResponse | null {
    return this.load()?.scoreResult ?? null;
  }

  setScoreResult(result: KibbeScoreResponse): void {
    const session = this.load() ?? {
      answers: {},
      scoreResult: null,
      startedAt: Date.now(),
    };
    session.scoreResult = result;
    this.save(session);
  }

  isExpired(): boolean {
    if (!this.isBrowser) return true;
    const raw = localStorage.getItem(SESSION_KEY);
    if (!raw) return true;
    try {
      const session: KibbeSession = JSON.parse(raw);
      return Date.now() - session.startedAt > SESSION_TTL_MS;
    } catch {
      return true;
    }
  }

  hasSession(): boolean {
    return this.load() !== null;
  }

  clear(): void {
    if (!this.isBrowser) return;
    localStorage.removeItem(SESSION_KEY);
  }
}
