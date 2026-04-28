import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class FollowService {
  private readonly api = inject(ApiService);
  private readonly followedUserIds = signal<Set<string>>(new Set());

  isFollowing(userId: string): boolean {
    return this.followedUserIds().has(userId);
  }

  follow(userId: string): void {
    this.followedUserIds.update(set => {
      const next = new Set(set);
      next.add(userId);
      return next;
    });

    this.api.post('users', `${userId}/follow`, {}).subscribe({
      error: () => {
        this.followedUserIds.update(set => {
          const next = new Set(set);
          next.delete(userId);
          return next;
        });
      },
    });
  }

  unfollow(userId: string): void {
    this.followedUserIds.update(set => {
      const next = new Set(set);
      next.delete(userId);
      return next;
    });

    this.api.delete('users', `${userId}/follow`, {}).subscribe({
      error: () => {
        this.followedUserIds.update(set => {
          const next = new Set(set);
          next.add(userId);
          return next;
        });
      },
    });
  }

  checkFollowStatus(userId: string): void {
    this.api.get<boolean>('users', `${userId}/follow-status`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.followedUserIds.update(set => {
            const next = new Set(set);
            next.add(userId);
            return next;
          });
        }
      },
    });
  }

  syncFromFeed(posts: Array<{ author: { userId: string }; isFollowing: boolean }>): void {
    const followed = new Set(this.followedUserIds());
    for (const post of posts) {
      if (post.isFollowing) {
        followed.add(post.author.userId);
      }
    }
    this.followedUserIds.set(followed);
  }
}
