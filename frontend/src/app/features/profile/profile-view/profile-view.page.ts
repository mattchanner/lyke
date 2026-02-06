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
  templateUrl: './profile-view.page.html',
  styleUrls: ['./profile-view.page.scss'],
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
