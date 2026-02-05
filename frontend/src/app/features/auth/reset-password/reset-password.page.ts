import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  FormBuilder,
  ReactiveFormsModule,
  Validators,
  AbstractControl,
  ValidationErrors,
} from '@angular/forms';
import { Router, ActivatedRoute, RouterLink } from '@angular/router';
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
  lockClosedOutline,
  checkmarkCircleOutline,
} from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core';

function passwordMatchValidator(
  control: AbstractControl
): ValidationErrors | null {
  const password = control.get('newPassword');
  const confirmPassword = control.get('confirmPassword');

  if (password && confirmPassword && password.value !== confirmPassword.value) {
    confirmPassword.setErrors({ passwordMismatch: true });
    return { passwordMismatch: true };
  }

  return null;
}

@Component({
  selector: 'app-reset-password',
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
        <ion-title>Set New Password</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <div class="reset-container">
        @if (!resetComplete()) {
          <div class="header-section">
            <h2>Create new password</h2>
            <p>Your new password must be at least 8 characters long.</p>
          </div>

          <form [formGroup]="form" (ngSubmit)="onSubmit()">
            <ion-list>
              <ion-item>
                <ion-icon name="lock-closed-outline" slot="start"></ion-icon>
                <ion-input
                  formControlName="newPassword"
                  [type]="showPassword() ? 'text' : 'password'"
                  placeholder="New Password"
                  autocomplete="new-password"
                ></ion-input>
                <ion-icon
                  [name]="showPassword() ? 'eye-off-outline' : 'eye-outline'"
                  slot="end"
                  (click)="togglePassword()"
                  class="password-toggle"
                ></ion-icon>
              </ion-item>
              @if (form.get('newPassword')?.touched && form.get('newPassword')?.errors?.['required']) {
                <ion-text color="danger" class="error-text">
                  <small>Password is required</small>
                </ion-text>
              }
              @if (form.get('newPassword')?.touched && form.get('newPassword')?.errors?.['minlength']) {
                <ion-text color="danger" class="error-text">
                  <small>Password must be at least 8 characters</small>
                </ion-text>
              }

              <ion-item>
                <ion-icon name="lock-closed-outline" slot="start"></ion-icon>
                <ion-input
                  formControlName="confirmPassword"
                  [type]="showConfirmPassword() ? 'text' : 'password'"
                  placeholder="Confirm New Password"
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
                Reset Password
              }
            </ion-button>
          </form>
        } @else {
          <div class="success-section">
            <ion-icon
              name="checkmark-circle-outline"
              color="success"
              class="success-icon"
            ></ion-icon>
            <h2>Password Reset Complete</h2>
            <p>Your password has been successfully reset. You can now sign in with your new password.</p>
            <ion-button expand="block" routerLink="/auth/login">
              Sign In
            </ion-button>
          </div>
        }
      </div>
    </ion-content>
  `,
  styles: [`
    .reset-container {
      max-width: 400px;
      margin: 0 auto;
      padding-top: 2rem;
    }

    .header-section {
      text-align: center;
      margin-bottom: 2rem;

      h2 {
        font-size: 1.5rem;
        font-weight: 700;
        margin-bottom: 0.75rem;
      }

      p {
        color: var(--ion-color-medium);
        line-height: 1.5;
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

    .success-section {
      text-align: center;

      .success-icon {
        font-size: 4rem;
        margin-bottom: 1.5rem;
      }

      h2 {
        font-size: 1.5rem;
        font-weight: 700;
        margin-bottom: 0.75rem;
      }

      p {
        color: var(--ion-color-medium);
        line-height: 1.5;
        margin-bottom: 2rem;
      }
    }
  `],
})
export class ResetPasswordPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);
  readonly showConfirmPassword = signal(false);
  readonly resetComplete = signal(false);

  private email = '';
  private token = '';

  readonly form = this.fb.nonNullable.group(
    {
      newPassword: ['', [Validators.required, Validators.minLength(8)]],
      confirmPassword: ['', [Validators.required]],
    },
    { validators: passwordMatchValidator }
  );

  constructor() {
    addIcons({ eyeOutline, eyeOffOutline, lockClosedOutline, checkmarkCircleOutline });
  }

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      this.email = params['email'] || '';
      this.token = params['token'] || '';

      if (!this.email || !this.token) {
        this.toast.error('Invalid reset link');
        this.router.navigate(['/auth/forgot-password']);
      }
    });
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

    this.authService
      .resetPassword({
        email: this.email,
        token: this.token,
        newPassword: this.form.value.newPassword!,
        confirmPassword: this.form.value.confirmPassword!,
      })
      .subscribe({
        next: () => {
          this.resetComplete.set(true);
          this.toast.success('Password reset successfully!');
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
