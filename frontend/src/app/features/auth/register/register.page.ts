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

interface InterceptedError {
  status?: number;
  message?: string;
}

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
  readonly errorMessage = signal<string>('');
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
    this.errorMessage.set('');
    this.clearEmailTakenError();

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
        this.toast.info('Welcome to LYKE');
        this.router.navigate([this.getPostRegisterRoute()]);
      },
      error: (error: InterceptedError) => {
        this.isLoading.set(false);
        this.handleRegisterError(error);
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
    this.errorMessage.set('');
    this.socialLoading.set(true);
    try {
      const { idToken } = await this.socialAuth.googleSignIn();
      this.authService.socialLogin({ provider: 'Google', idToken }).subscribe({
        next: () => {
          this.router.navigate(['/feed']);
        },
        error: (error: InterceptedError) => {
          this.socialLoading.set(false);
          this.handleRegisterError(error);
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
          this.router.navigate(['/feed']);
        },
        error: (error: InterceptedError) => {
          this.socialLoading.set(false);
          this.handleRegisterError(error);
        },
        complete: () => this.socialLoading.set(false),
      });
    } catch (error) {
      this.socialLoading.set(false);
      this.errorMessage.set('Apple sign-in was cancelled or failed. Please try again.');
    }
  }

  private handleRegisterError(error: InterceptedError): void {
    if (error?.status === 409) {
      const emailControl = this.form.get('email');
      emailControl?.setErrors({ emailTaken: true });
      emailControl?.markAsTouched();
      this.errorMessage.set('');
      return;
    }
    this.errorMessage.set(this.formatRegisterError(error?.status, error?.message));
  }

  private formatRegisterError(status: number | undefined, message: string | undefined): string {
    switch (status) {
      case 0:
        return "We couldn't reach LYKE. Check your connection and try again.";
      case 422:
        return message || 'Some fields need attention. Please review and try again.';
      case 429:
        return 'Too many attempts. Please wait a moment and try again.';
      case 500:
      case 502:
      case 503:
      case 504:
        return 'Sign up is temporarily unavailable. Please try again in a moment.';
      default:
        return 'Sign up failed. Please try again.';
    }
  }

  private clearEmailTakenError(): void {
    const emailControl = this.form.get('email');
    if (emailControl?.errors?.['emailTaken']) {
      const { emailTaken, ...rest } = emailControl.errors;
      emailControl.setErrors(Object.keys(rest).length ? rest : null);
    }
  }
}
