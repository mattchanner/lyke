import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
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
  IonBackButton,
  IonButtons,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  eyeOutline,
  eyeOffOutline,
  mailOutline,
  lockClosedOutline,
} from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core';
import { RegisterRequest, UserType } from '../../../models';

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
  ],
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-buttons slot="start">
          <ion-back-button defaultHref="/auth/login"></ion-back-button>
        </ion-buttons>
        <ion-title>Create Account</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <div class="register-container">
        <div class="header-section">
          <h2>Join LYKE</h2>
          <p>Start discovering fashion that fits your body</p>
        </div>

        <form [formGroup]="form" (ngSubmit)="onSubmit()">
          <ion-list>
            <ion-item>
              <ion-icon name="mail-outline" slot="start"></ion-icon>
              <ion-input
                formControlName="email"
                type="email"
                placeholder="Email"
                autocomplete="email"
              ></ion-input>
            </ion-item>
            @if (form.get('email')?.touched && form.get('email')?.errors?.['required']) {
              <ion-text color="danger" class="error-text">
                <small>Email is required</small>
              </ion-text>
            }
            @if (form.get('email')?.touched && form.get('email')?.errors?.['email']) {
              <ion-text color="danger" class="error-text">
                <small>Please enter a valid email</small>
              </ion-text>
            }

            <ion-item>
              <ion-icon name="lock-closed-outline" slot="start"></ion-icon>
              <ion-input
                formControlName="password"
                [type]="showPassword() ? 'text' : 'password'"
                placeholder="Password"
                autocomplete="new-password"
              ></ion-input>
              <ion-icon
                [name]="showPassword() ? 'eye-off-outline' : 'eye-outline'"
                slot="end"
                (click)="togglePassword()"
                class="password-toggle"
              ></ion-icon>
            </ion-item>
            @if (form.get('password')?.touched && form.get('password')?.errors?.['required']) {
              <ion-text color="danger" class="error-text">
                <small>Password is required</small>
              </ion-text>
            }
            @if (form.get('password')?.touched && form.get('password')?.errors?.['minlength']) {
              <ion-text color="danger" class="error-text">
                <small>Password must be at least 8 characters</small>
              </ion-text>
            }

            <ion-item>
              <ion-icon name="lock-closed-outline" slot="start"></ion-icon>
              <ion-input
                formControlName="confirmPassword"
                [type]="showConfirmPassword() ? 'text' : 'password'"
                placeholder="Confirm Password"
                autocomplete="new-password"
              ></ion-input>
              <ion-icon
                [name]="showConfirmPassword() ? 'eye-off-outline' : 'eye-outline'"
                slot="end"
                (click)="toggleConfirmPassword()"
                class="password-toggle"
              ></ion-icon>
            </ion-item>
            @if (form.get('confirmPassword')?.touched && form.get('confirmPassword')?.errors?.['required']) {
              <ion-text color="danger" class="error-text">
                <small>Please confirm your password</small>
              </ion-text>
            }
            @if (form.get('confirmPassword')?.touched && form.get('confirmPassword')?.errors?.['passwordMismatch']) {
              <ion-text color="danger" class="error-text">
                <small>Passwords do not match</small>
              </ion-text>
            }
          </ion-list>

          <ion-button
            expand="block"
            type="submit"
            [disabled]="form.invalid || isLoading()"
          >
            @if (isLoading()) {
              <ion-spinner name="crescent"></ion-spinner>
            } @else {
              Create Account
            }
          </ion-button>
        </form>

        <div class="terms-text">
          <small>
            By signing up, you agree to our
            <a href="#">Terms of Service</a> and
            <a href="#">Privacy Policy</a>
          </small>
        </div>

        <div class="login-link">
          <p>Already have an account? <a routerLink="/auth/login">Sign in</a></p>
        </div>
      </div>
    </ion-content>
  `,
  styles: [`
    .register-container {
      max-width: 400px;
      margin: 0 auto;
    }

    .header-section {
      text-align: center;
      margin-bottom: 2rem;

      h2 {
        font-size: 1.75rem;
        font-weight: 700;
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--ion-color-medium);
      }
    }

    ion-list {
      background: transparent;
      margin-bottom: 1rem;
    }

    ion-item {
      --background: var(--ion-color-light);
      --border-radius: 8px;
      margin-bottom: 0.5rem;
    }

    .error-text {
      display: block;
      padding-left: 1rem;
      margin-bottom: 0.5rem;
    }

    .password-toggle {
      cursor: pointer;
    }

    ion-button {
      --border-radius: 8px;
      margin-top: 1rem;
    }

    .terms-text {
      text-align: center;
      margin-top: 1.5rem;
      color: var(--ion-color-medium);

      a {
        color: var(--ion-color-primary);
        text-decoration: none;
      }
    }

    .login-link {
      text-align: center;
      margin-top: 2rem;

      a {
        color: var(--ion-color-primary);
        text-decoration: none;
        font-weight: 600;
      }
    }
  `],
})
export class RegisterPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator }
  );

  constructor() {
    addIcons({ eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline });
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
    };

    this.authService.register(request).subscribe({
      next: () => {
        this.toast.success('Account created successfully!');
        this.router.navigate(['/onboarding']);
      },
      error: () => {
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }
}
