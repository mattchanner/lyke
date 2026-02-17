import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
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
import { AuthService, ToastService, SocialAuthService } from '../../../core';
import { LoginRequest, UserType } from '../../../models';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
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
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly socialLoading = signal(false);
  readonly showPassword = signal(false);
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
        this.toast.success('Welcome back!');
        this.router.navigate([this.getPostLoginRoute()]);
      },
      error: (error) => {
        this.isLoading.set(false);
        // Error toast is handled by error interceptor
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  async onGoogleSignIn(): Promise<void> {
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.googleSignIn();
      this.authService.socialLogin({ provider: 'Google', idToken }).subscribe({
        next: () => {
          this.toast.success('Welcome back!');
          this.router.navigate([this.getPostLoginRoute()]);
        },
        error: () => this.socialLoading.set(false),
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.toast.error('Google sign-in failed');
    }
  }

  private getPostLoginRoute(): string {
    const userType = this.authService.userType();
    if (userType === UserType.Admin) return '/admin';
    if (userType === UserType.Creator) return '/creator';
    if (userType === UserType.Retailer) return '/retailer';
    return '/feed';
  }

  async onAppleSignIn(): Promise<void> {
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.appleSignIn();
      this.authService.socialLogin({ provider: 'Apple', idToken }).subscribe({
        next: () => {
          this.toast.success('Welcome back!');
          this.router.navigate([this.getPostLoginRoute()]);
        },
        error: () => this.socialLoading.set(false),
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.toast.error('Apple sign-in failed');
    }
  }
}
