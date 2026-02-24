import { Component, inject, signal, OnInit, CUSTOM_ELEMENTS_SCHEMA } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonAvatar,
  IonIcon,
  IonChip,
  IonLabel,
  IonButton,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { checkmarkCircle } from 'ionicons/icons';
import { ApiService, FollowService } from '../../../core';
import { FeedPostResponse, PublicCreatorProfileResponse } from '../../../models';
import { PostCardComponent } from '../../../shared/components/post-card/post-card.component';

@Component({
  selector: 'app-creator-profile',
  standalone: true,
  imports: [
    CommonModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonAvatar,
    IonIcon,
    IonChip,
    IonLabel,
    IonButton,
    PostCardComponent,
  ],
  schemas: [CUSTOM_ELEMENTS_SCHEMA],
  templateUrl: './creator-profile.page.html',
  styleUrls: ['./creator-profile.page.scss'],
})
export class CreatorProfilePage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly api = inject(ApiService);
  readonly followService = inject(FollowService);

  readonly creator = signal<PublicCreatorProfileResponse | null>(null);
  readonly posts = signal<FeedPostResponse[]>([]);
  readonly isLoading = signal(false);
  readonly isLoadingPosts = signal(false);

  constructor() {
    addIcons({ checkmarkCircle });
  }

  ngOnInit(): void {
    const creatorId = this.route.snapshot.params['id'];
    if (creatorId) {
      this.loadCreator(creatorId);
      this.loadCreatorPosts(creatorId);
      this.followService.checkFollowStatus(creatorId);
    }
  }

  toggleFollow(): void {
    const c = this.creator();
    if (!c) return;
    if (this.followService.isFollowing(c.id)) {
      this.followService.unfollow(c.id);
    } else {
      this.followService.follow(c.id);
    }
  }

  private loadCreator(creatorId: string): void {
    this.isLoading.set(true);

    this.api
      .get<PublicCreatorProfileResponse>('creators', `${creatorId}/public`)
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.creator.set(response.data);
          }
        },
        complete: () => this.isLoading.set(false),
      });
  }

  private loadCreatorPosts(creatorId: string): void {
    this.isLoadingPosts.set(true);

    this.api
      .get<FeedPostResponse[]>('feed', '', { creatorId })
      .subscribe({
        next: (response) => {
          if (response.success && response.data) {
            this.posts.set(response.data);
          }
        },
        complete: () => this.isLoadingPosts.set(false),
      });
  }
}
