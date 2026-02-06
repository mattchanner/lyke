import { Component, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonIcon,
  IonInput,
  IonTextarea,
  IonItem,
  IonLabel,
  IonList,
  IonListHeader,
  IonSelect,
  IonSelectOption,
  IonSpinner,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  addCircleOutline,
  trashOutline,
  rocketOutline,
} from 'ionicons/icons';
import { CreatorService, AuthService, ToastService } from '../../../core';
import { RegisterCreatorRequest } from '../../../models';

interface SocialLink {
  platform: string;
  url: string;
}

@Component({
  selector: 'app-creator-register',
  standalone: true,
  imports: [
    FormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonIcon,
    IonInput,
    IonTextarea,
    IonItem,
    IonLabel,
    IonList,
    IonListHeader,
    IonSelect,
    IonSelectOption,
    IonSpinner,
  ],
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
})
export class RegisterPage {
  private readonly creatorService = inject(CreatorService);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);
  private readonly router = inject(Router);

  readonly displayName = signal('');
  readonly bio = signal('');
  readonly socialLinks = signal<SocialLink[]>([]);
  readonly isSubmitting = signal(false);

  readonly platforms = [
    'Instagram',
    'TikTok',
    'YouTube',
    'Twitter',
    'Pinterest',
  ];

  constructor() {
    addIcons({ addCircleOutline, trashOutline, rocketOutline });
  }

  addSocialLink(): void {
    this.socialLinks.update((links) => [
      ...links,
      { platform: '', url: '' },
    ]);
  }

  removeSocialLink(index: number): void {
    this.socialLinks.update((links) => links.filter((_, i) => i !== index));
  }

  updatePlatform(index: number, platform: string): void {
    this.socialLinks.update((links) =>
      links.map((link, i) => (i === index ? { ...link, platform } : link))
    );
  }

  updateUrl(index: number, url: string): void {
    this.socialLinks.update((links) =>
      links.map((link, i) => (i === index ? { ...link, url } : link))
    );
  }

  submit(): void {
    const name = this.displayName().trim();
    if (!name) {
      this.toast.error('Display name is required');
      return;
    }

    this.isSubmitting.set(true);

    const socialLinksMap: Record<string, string> = {};
    for (const link of this.socialLinks()) {
      if (link.platform && link.url.trim()) {
        socialLinksMap[link.platform] = link.url.trim();
      }
    }

    const request: RegisterCreatorRequest = {
      displayName: name,
      ...(this.bio().trim() && { bio: this.bio().trim() }),
      ...(Object.keys(socialLinksMap).length > 0 && {
        socialLinks: socialLinksMap,
      }),
    };

    this.creatorService.register(request).subscribe({
      next: (response) => {
        if (response.success) {
          this.authService.refreshSession().subscribe({
            next: () => {
              this.toast.success('Welcome, Creator!');
              this.router.navigate(['/creator']);
            },
            error: () => {
              this.toast.success('Creator account created! Please log in again.');
              this.authService.logout();
            },
          });
        } else {
          this.toast.error(response.error?.message ?? 'Registration failed');
          this.isSubmitting.set(false);
        }
      },
      error: (err) => {
        this.toast.error(err.message ?? 'Something went wrong');
        this.isSubmitting.set(false);
      },
    });
  }
}
