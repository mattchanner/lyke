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
import { eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline } from 'ionicons/icons';
import { AuthService, ToastService } from '../../../core';
import { LoginRequest } from '../../../models';

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
  template: `
    <ion-content class="ion-padding">
      <div class="login-container">
        <div class="logo-section">
          <h1>LYKE</h1>
          <p>Fashion that fits your body</p>
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
                autocomplete="current-password"
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
          </ion-list>

          <div class="forgot-password">
            <a routerLink="/auth/forgot-password">Forgot password?</a>
          </div>

          <ion-button
            expand="block"
            type="submit"
            [disabled]="form.invalid || isLoading()"
          >
            @if (isLoading()) {
              <ion-spinner name="crescent"></ion-spinner>
            } @else {
              Sign In
            }
          </ion-button>
        </form>

        <div class="register-link">
          <p>Don't have an account? <a routerLink="/auth/register">Sign up</a></p>
        </div>
      </div>
    </ion-content>
  `,
  styles: [`
    .login-container {
      display: flex;
      flex-direction: column;
      justify-content: center;
      min-height: 100%;
      max-width: 400px;
      margin: 0 auto;
    }

    .logo-section {
      text-align: center;
      margin-bottom: 2rem;

      h1 {
        font-size: 2.5rem;
        font-weight: 700;
        margin-bottom: 0.5rem;
        color: var(--ion-color-primary);
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

    .forgot-password {
      text-align: right;
      margin-bottom: 1.5rem;

      a {
        color: var(--ion-color-primary);
        text-decoration: none;
        font-size: 0.9rem;
      }
    }

    ion-button {
      --border-radius: 8px;
      margin-top: 1rem;
    }

    .register-link {
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
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly isLoading = signal(false);
  readonly showPassword = signal(false);

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]],
  });

  constructor() {
    addIcons({ eyeOutline, eyeOffOutline, mailOutline, lockClosedOutline });
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
        this.router.navigate(['/feed']);
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
}
