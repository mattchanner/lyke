import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';

export interface EngagementChange {
  postId: string;
  type: 'like' | 'save';
  state: boolean;
}

@Injectable({
  providedIn: 'root',
})
export class PostEngagementService {
  private readonly changes$ = new Subject<EngagementChange>();
  readonly engagementChanged = this.changes$.asObservable();

  notify(change: EngagementChange): void {
    this.changes$.next(change);
  }
}
