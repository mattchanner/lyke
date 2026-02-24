import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonList,
  IonItem,
  IonLabel,
  IonAvatar,
  IonIcon,
  IonButton,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { checkmarkCircle } from 'ionicons/icons';
import { ApiService, FollowService } from '../../../core';
import { FollowedCreatorResponse } from '../../../models';

@Component({
  selector: 'app-following',
  standalone: true,
  imports: [
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonList,
    IonItem,
    IonLabel,
    IonAvatar,
    IonIcon,
    IonButton,
  ],
  templateUrl: './following.page.html',
  styleUrls: ['./following.page.scss'],
})
export class FollowingPage implements OnInit {
  private readonly api = inject(ApiService);
  readonly followService = inject(FollowService);

  readonly creators = signal<FollowedCreatorResponse[]>([]);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({ checkmarkCircle });
  }

  ngOnInit(): void {
    this.loadFollowing();
  }

  loadFollowing(): void {
    this.isLoading.set(true);

    this.api.get<FollowedCreatorResponse[]>('users', 'following').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.creators.set(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  unfollow(creatorId: string): void {
    this.followService.unfollow(creatorId);
    this.creators.update(list => list.filter(c => c.creatorId !== creatorId));
  }
}
