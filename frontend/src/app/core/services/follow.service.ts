import { Injectable, inject, signal } from '@angular/core';
import { ApiService } from './api.service';

@Injectable({ providedIn: 'root' })
export class FollowService {
  private readonly api = inject(ApiService);
  private readonly followedCreatorIds = signal<Set<string>>(new Set());

  isFollowing(creatorId: string): boolean {
    return this.followedCreatorIds().has(creatorId);
  }

  follow(creatorId: string): void {
    this.followedCreatorIds.update(set => {
      const next = new Set(set);
      next.add(creatorId);
      return next;
    });

    this.api.post('creators', `${creatorId}/follow`, {}).subscribe({
      error: () => {
        this.followedCreatorIds.update(set => {
          const next = new Set(set);
          next.delete(creatorId);
          return next;
        });
      },
    });
  }

  unfollow(creatorId: string): void {
    this.followedCreatorIds.update(set => {
      const next = new Set(set);
      next.delete(creatorId);
      return next;
    });

    this.api.delete('creators', `${creatorId}/follow`, {}).subscribe({
      error: () => {
        this.followedCreatorIds.update(set => {
          const next = new Set(set);
          next.add(creatorId);
          return next;
        });
      },
    });
  }

  checkFollowStatus(creatorId: string): void {
    this.api.get<boolean>('creators', `${creatorId}/follow-status`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.followedCreatorIds.update(set => {
            const next = new Set(set);
            next.add(creatorId);
            return next;
          });
        }
      },
    });
  }

  syncFromFeed(posts: Array<{ creator: { id: string }; isFollowing: boolean }>): void {
    const followed = new Set(this.followedCreatorIds());
    for (const post of posts) {
      if (post.isFollowing) {
        followed.add(post.creator.id);
      }
    }
    this.followedCreatorIds.set(followed);
  }
}
