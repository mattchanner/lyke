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
import { FollowedUserResponse } from '../../../models';

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

  readonly users = signal<FollowedUserResponse[]>([]);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({ checkmarkCircle });
  }

  ngOnInit(): void {
    this.loadFollowing();
  }

  loadFollowing(): void {
    this.isLoading.set(true);

    this.api.get<FollowedUserResponse[]>('users', 'following').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.users.set(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  unfollow(userId: string): void {
    this.followService.unfollow(userId);
    this.users.update(list => list.filter(u => u.userId !== userId));
  }
}
