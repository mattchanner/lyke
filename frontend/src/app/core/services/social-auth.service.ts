import { Injectable } from '@angular/core';
import { Capacitor } from '@capacitor/core';
import { SocialLogin } from '@capgo/capacitor-social-login';
import { environment } from '../../../environments/environment';

declare const google: {
  accounts: {
    id: {
      initialize(config: {
        client_id: string;
        callback: (response: { credential: string }) => void;
        auto_select?: boolean;
        cancel_on_tap_outside?: boolean;
      }): void;
      prompt(callback?: (notification: { isNotDisplayed(): boolean; isSkippedMoment(): boolean }) => void): void;
    };
  };
};

@Injectable({
  providedIn: 'root',
})
export class SocialAuthService {
  private initialized = false;
  private gisLoaded = false;

  async initialize(): Promise<void> {
    if (this.initialized) return;

    if (Capacitor.isNativePlatform()) {
      await SocialLogin.initialize({
        google: { webClientId: environment.googleClientId },
        apple: { clientId: environment.appleClientId },
      });
    }

    this.initialized = true;
  }

  async googleSignIn(): Promise<{ idToken: string }> {
    await this.initialize();

    if (!Capacitor.isNativePlatform()) {
      return this.googleSignInWeb();
    }

    return this.googleSignInNative();
  }

  async appleSignIn(): Promise<{ idToken: string }> {
    await this.initialize();

    const response = await SocialLogin.login({
      provider: 'apple',
      options: { scopes: ['email', 'name'] },
    });

    const idToken = response.result.idToken;
    if (!idToken) throw new Error('No ID token received from Apple');

    return { idToken };
  }

  private async googleSignInNative(): Promise<{ idToken: string }> {
    const response = await SocialLogin.login({
      provider: 'google',
      options: { scopes: ['email', 'profile'] },
    });

    if (response.result.responseType === 'offline') {
      throw new Error('Unexpected offline response from Google');
    }

    const idToken = response.result.idToken;
    if (!idToken) throw new Error('No ID token received from Google');

    return { idToken };
  }

  private async googleSignInWeb(): Promise<{ idToken: string }> {
    await this.loadGisScript();

    return new Promise((resolve, reject) => {
      google.accounts.id.initialize({
        client_id: environment.googleClientId,
        callback: (response) => {
          if (response.credential) {
            resolve({ idToken: response.credential });
          } else {
            reject(new Error('No credential received from Google'));
          }
        },
        cancel_on_tap_outside: true,
      });

      google.accounts.id.prompt((notification) => {
        if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
          reject(new Error('Google sign-in was dismissed'));
        }
      });
    });
  }

  private loadGisScript(): Promise<void> {
    if (this.gisLoaded) return Promise.resolve();

    return new Promise((resolve, reject) => {
      if (typeof google !== 'undefined' && google.accounts?.id) {
        this.gisLoaded = true;
        resolve();
        return;
      }

      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;
      script.onload = () => {
        this.gisLoaded = true;
        resolve();
      };
      script.onerror = () => reject(new Error('Failed to load Google Identity Services'));
      document.head.appendChild(script);
    });
  }
}
