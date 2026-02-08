import { Injectable } from '@angular/core';
import { SocialLogin } from '@capgo/capacitor-social-login';
import { environment } from '../../../environments/environment';

@Injectable({
  providedIn: 'root',
})
export class SocialAuthService {
  private initialized = false;

  async initialize(): Promise<void> {
    if (this.initialized) return;

    await SocialLogin.initialize({
      google: {
        webClientId: environment.googleClientId,
      },
      apple: {
        clientId: environment.appleClientId,
      },
    });

    this.initialized = true;
  }

  async googleSignIn(): Promise<{ idToken: string }> {
    await this.initialize();

    const response = await SocialLogin.login({
      provider: 'google',
      options: {
        scopes: ['email', 'profile'],
      },
    });

    if (response.result.responseType === 'offline') {
      throw new Error('Unexpected offline response from Google');
    }

    const idToken = response.result.idToken;
    if (!idToken) {
      throw new Error('No ID token received from Google');
    }

    return { idToken };
  }

  async appleSignIn(): Promise<{ idToken: string }> {
    await this.initialize();

    const response = await SocialLogin.login({
      provider: 'apple',
      options: {
        scopes: ['email', 'name'],
      },
    });

    const idToken = response.result.idToken;
    if (!idToken) {
      throw new Error('No ID token received from Apple');
    }

    return { idToken };
  }
}
