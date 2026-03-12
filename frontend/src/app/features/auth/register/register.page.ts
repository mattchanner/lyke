import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
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
  IonBackButton,
  IonButtons,
  IonCheckbox,
  IonLabel,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  eyeOutline,
  eyeOffOutline,
  mailOutline,
  lockClosedOutline,
  logoGoogle,
  logoApple,
} from 'ionicons/icons';
import { AuthService, ToastService, SocialAuthService, KibbeAnalyticsService } from '../../../core';
import { RegisterRequest, UserType } from '../../../models';
import { environment } from '../../../../environments/environment';

function passwordMatchValidator(
  control: AbstractControl
): ValidationErrors | null {
  const password = control.get('password');
  const confirmPassword = control.get('confirmPassword');

  if (password && confirmPassword && password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  }

  return null;
}

@Component({
  selector: 'app-register',
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
    IonBackButton,
    IonButtons,
    IonCheckbox,
    IonLabel,
  ],
  templateUrl: './register.page.html',
  styleUrls: ['./register.page.scss'],
})
export class RegisterPage {
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
  readonly showConfirmPassword = signal(false);
  readonly socialLoginsEnabled = environment.socialLoginsEnabled;

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
      acceptPrivacyPolicy: [false, [Validators.requiredTrue]],
    },
    { validators: passwordMatchValidator }
  );

  constructor() {
    addIcons({ eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline, logoGoogle, logoApple });
  }

  togglePassword(): void {
    this.showPassword.update((v) => !v);
  }

  toggleConfirmPassword(): void {
    this.showConfirmPassword.update((v) => !v);
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    const request: RegisterRequest = {
      email: this.form.value.email!,
      password: this.form.value.password!,
      confirmPassword: this.form.value.confirmPassword!,
      userType: UserType.Shopper,
      acceptPrivacyPolicy: true,
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.toast.success('Account created successfully!');
        this.router.navigate([this.getPostRegisterRoute()]);
      },
      error: () => {
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  private getPostRegisterRoute(): string {
    const returnPath = this.route.snapshot.queryParamMap.get('return');
    if (returnPath) {
      this.kibbeAnalytics.registerCompleteFromQuiz();
      return `/${returnPath}`;
    }
    return '/onboarding';
  }

  async onGoogleSignIn(): Promise<void> {
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.googleSignIn();
      this.authService.socialLogin({ provider: 'Google', idToken }).subscribe({
        next: () => {
          this.toast.success('Account created successfully!');
          this.router.navigate(['/feed']);
        },
        error: () => this.socialLoading.set(false),
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.toast.error('Google sign-in failed');
    }
  }

  async onAppleSignIn(): Promise<void> {
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.appleSignIn();
      this.authService.socialLogin({ provider: 'Apple', idToken }).subscribe({
        next: () => {
          this.toast.success('Account created successfully!');
          this.router.navigate(['/feed']);
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
