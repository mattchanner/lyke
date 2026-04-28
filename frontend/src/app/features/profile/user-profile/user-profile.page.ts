import { Component, inject, signal, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonIcon,
  IonAvatar,
  IonSpinner,
  IonChip,
  IonLabel,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { checkmarkCircle } from 'ionicons/icons';
import { ApiService, FollowService } from '../../../core';
import { PublicUserProfileResponse, FeedPostResponse } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card';

@Component({
  selector: 'app-user-profile',
  standalone: true,
  imports: [
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonIcon,
    IonAvatar,
    IonSpinner,
    IonChip,
    IonLabel,
    PostCardComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './user-profile.page.html',
  styleUrls: ['./user-profile.page.scss'],
})
export class UserProfilePage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  readonly followService = inject(FollowService);

  readonly profile = signal<PublicUserProfileResponse | null>(null);
  readonly posts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({ checkmarkCircle });
  }

  ngOnInit(): void {
    const userId = this.route.snapshot.paramMap.get('id');
    if (userId) {
      this.loadProfile(userId);
    }
  }

  loadProfile(userId: string): void {
    this.isLoading.set(true);

    this.api.get<PublicUserProfileResponse>('users', `${userId}/profile`).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.profile.set(response.data);
          this.followService.checkFollowStatus(userId);
          this.loadPosts(userId);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  private loadPosts(userId: string): void {
    this.api.get<FeedPostResponse[]>('feed', '', { authorUserId: userId }).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.posts.set(response.data);
        }
      },
    });
  }

  toggleFollow(): void {
    const p = this.profile();
    if (!p) return;

    if (this.followService.isFollowing(p.userId)) {
      this.followService.unfollow(p.userId);
    } else {
      this.followService.follow(p.userId);
    }
  }
}
