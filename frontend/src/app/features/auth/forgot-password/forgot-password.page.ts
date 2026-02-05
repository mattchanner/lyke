import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { RouterLink } from '@angular/router';
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
import { mailOutline, checkmarkCircleOutline } from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core';

@Component({
  selector: 'app-forgot-password',
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
        <ion-title>Reset Password</ion-title>
      </ion-toolbar>
    </ion-header>

    <ion-content class="ion-padding">
      <div class="forgot-container">
        @if (!emailSent()) {
          <div class="header-section">
            <h2>Forgot your password?</h2>
            <p>
              Enter your email address and we'll send you instructions to reset
              your password.
            </p>
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
            </ion-list>

            <ion-button
              expand="block"
              type="submit"
              [disabled]="form.invalid || isLoading()"
            >
              @if (isLoading()) {
                <ion-spinner name="crescent"></ion-spinner>
              } @else {
                Send Reset Link
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
            <h2>Check your email</h2>
            <p>
              We've sent password reset instructions to
              <strong>{{ form.value.email }}</strong>
            </p>
            <ion-button expand="block" routerLink="/auth/login">
              Back to Login
            </ion-button>
            <div class="resend-link">
              <p>
                Didn't receive the email?
                <a (click)="onSubmit()">Resend</a>
              </p>
            </div>
          </div>
        }
      </div>
    </ion-content>
  `,
  styles: [`
    .forgot-container {
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

    .resend-link {
      margin-top: 1.5rem;

      a {
        color: var(--ion-color-primary);
        text-decoration: none;
        font-weight: 600;
        cursor: pointer;
      }
    }
  `],
})
export class ForgotPasswordPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly emailSent = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
  });

  constructor() {
    addIcons({ mailOutline, checkmarkCircleOutline });
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading.set(true);

    this.authService.forgotPassword({ email: this.form.value.email! }).subscribe({
      next: () => {
        this.emailSent.set(true);
        this.toast.success('Reset instructions sent!');
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
