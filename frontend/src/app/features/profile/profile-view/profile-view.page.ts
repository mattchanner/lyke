import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonButton,
  IonIcon,
  IonButtons,
  IonSpinner,
  IonList,
  IonItem,
  IonLabel,
  IonAvatar,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  settingsOutline,
  createOutline,
  bodyOutline,
  personOutline,
  logOutOutline,
  ribbonOutline,
} from 'ionicons/icons';
import { ApiService, AuthService } from '../../../core';
import { UserProfileResponse, BodyProfileResponse } from '../../../models';
import { SkeletonProfileComponent } from '../../../shared/components/loading-skeleton';

@Component({
  selector: 'app-profile-view',
  standalone: true,
  imports: [
    CommonModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonButton,
    IonIcon,
    IonButtons,
    IonSpinner,
    IonList,
    IonItem,
    IonLabel,
    IonAvatar,
    SkeletonProfileComponent,
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-title>Profile</ion-title>
        <ion-buttons slot="end">
          <ion-button routerLink="/settings">
            <ion-icon name="settings-outline" slot="icon-only"></ion-icon>
          </ion-button>
        </ion-buttons>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      @if (isLoading()) {
        <app-skeleton-profile></app-skeleton-profile>
      } @else if (profile()) {
        <div class="profile-header">
          <ion-avatar>
            <img src="assets/default-avatar.svg" alt="Profile" />
          </ion-avatar>
          <h2>{{ profile()!.email }}</h2>
          <p>Member since {{ profile()!.createdAt | date:'mediumDate' }}</p>
        </div>

        <ion-list>
          <ion-item routerLink="/profile/edit" detail>
            <ion-icon name="person-outline" slot="start"></ion-icon>
            <ion-label>Edit Profile</ion-label>
          </ion-item>

          <ion-item routerLink="/profile/body-profile" detail>
            <ion-icon name="body-outline" slot="start"></ion-icon>
            <ion-label>Body Profile</ion-label>
            @if (bodyProfile()) {
              <ion-label slot="end" color="medium">
                {{ bodyProfile()!.heightDisplay }} · {{ bodyProfile()!.weightDisplay }}
              </ion-label>
            }
          </ion-item>

          @if (authService.isCreator()) {
            <ion-item routerLink="/creator" detail>
              <ion-icon name="ribbon-outline" slot="start"></ion-icon>
              <ion-label>Creator Dashboard</ion-label>
            </ion-item>
          }

          <ion-item (click)="logout()" button>
            <ion-icon name="log-out-outline" slot="start" color="danger"></ion-icon>
            <ion-label color="danger">Log Out</ion-label>
          </ion-item>
        </ion-list>
      }
    </ion-content>
  `,
  styles: [`
    .profile-header {
      text-align: center;
      padding: 2rem 0;

      ion-avatar {
        width: 100px;
        height: 100px;
        margin: 0 auto 1rem;
      }

      h2 {
        font-size: 1.25rem;
        margin-bottom: 0.25rem;
      }

      p {
        color: var(--ion-color-medium);
        font-size: 0.9rem;
      }
    }
  `],
})
export class ProfileViewPage implements OnInit {
  private readonly api = inject(ApiService);
  readonly authService = inject(AuthService);

  readonly profile = signal<UserProfileResponse | null>(null);
  readonly bodyProfile = signal<BodyProfileResponse | null>(null);
  readonly isLoading = signal(false);

  constructor() {
    addIcons({
      settingsOutline,
      createOutline,
      bodyOutline,
      personOutline,
      logOutOutline,
      ribbonOutline,
    });
  }

  ngOnInit(): void {
    this.loadProfile();
  }

  loadProfile(): void {
    this.isLoading.set(true);

    this.api.get<UserProfileResponse>('profile', 'me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.profile.set(response.data);
          if (response.data.hasBodyProfile) {
            this.loadBodyProfile();
          }
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  loadBodyProfile(): void {
    this.api.get<BodyProfileResponse>('profile', 'body').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.bodyProfile.set(response.data);
        }
      },
    });
  }

  logout(): void {
    this.authService.logout();
  }
}
