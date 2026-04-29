import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import {
  IonContent,
  IonButton,
  IonInput,
  IonItem,
  IonList,
  IonText,
  IonSpinner,
  IonIcon,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline, logoGoogle, logoApple } from 'ionicons/icons';
import { AuthService, ToastService, SocialAuthService, KibbeAnalyticsService } from '../../../core';
import { LoginRequest, UserType } from '../../../models';
import { environment } from '../../../../environments/environment';

interface InterceptedError {
  status?: number;
  message?: string;
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    IonContent,
    IonButton,
    IonInput,
    IonItem,
    IonList,
    IonText,
    IonSpinner,
    IonIcon,
  ],
  templateUrl: './login.page.html',
  styleUrls: ['./login.page.scss'],
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly socialAuth = inject(SocialAuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);
  private readonly kibbeAnalytics = inject(KibbeAnalyticsService);

  readonly isLoading = signal(false);
  readonly socialLoading = signal(false);
  readonly showPassword = signal(false);
  readonly errorMessage = signal<string>('');
  readonly socialLoginsEnabled = environment.socialLoginsEnabled;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  constructor() {
    addIcons({ eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline, logoGoogle, logoApple });
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  onSubmit(): void {
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    const request: LoginRequest = {
      email: this.form.value.email!,
      password: this.form.value.password!,
    };

    this.authService.login(request).subscribe({
      next: () => {
        this.router.navigate([this.getPostLoginRoute()]);
      },
      error: (error: InterceptedError) => {
        this.isLoading.set(false);
        this.errorMessage.set(this.formatLoginError(error?.status));
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  async onGoogleSignIn(): Promise<void> {
    this.errorMessage.set('');
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.googleSignIn();
      this.authService.socialLogin({ provider: 'Google', idToken }).subscribe({
        next: () => {
          this.router.navigate([this.getPostLoginRoute()]);
        },
        error: (error: InterceptedError) => {
          this.socialLoading.set(false);
          this.errorMessage.set(this.formatLoginError(error?.status));
        },
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.errorMessage.set('Google sign-in was cancelled or failed. Please try again.');
    }
  }

  async onAppleSignIn(): Promise<void> {
    this.errorMessage.set('');
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.appleSignIn();
      this.authService.socialLogin({ provider: 'Apple', idToken }).subscribe({
        next: () => {
          this.router.navigate([this.getPostLoginRoute()]);
        },
        error: (error: InterceptedError) => {
          this.socialLoading.set(false);
          this.errorMessage.set(this.formatLoginError(error?.status));
        },
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.errorMessage.set('Apple sign-in was cancelled or failed. Please try again.');
    }
  }

  private formatLoginError(status: number | undefined): string {
    switch (status) {
      case 401:
        return 'That email and password combination is not recognised. Please try again.';
      case 0:
        return "We couldn't reach LYKE. Check your connection and try again.";
      case 429:
        return 'Too many sign-in attempts. Please wait a moment and try again.';
      case 500:
      case 502:
      case 503:
      case 504:
        return 'Sign in is temporarily unavailable. Please try again in a moment.';
      default:
        return 'Sign in failed. Please try again.';
    }
  }

  private getPostLoginRoute(): string {
    const returnPath = this.route.snapshot.queryParamMap.get('return');
    if (returnPath) {
      this.kibbeAnalytics.loginCompleteFromQuiz();
      return `/${returnPath}`;
    }
    const userType = this.authService.userType();
    if (userType === UserType.Admin) return '/admin';
    if (userType === UserType.Creator) return '/creator';
    if (userType === UserType.Retailer) return '/retailer';
    return '/feed';
  }
}
