import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { Preferences } from '@capacitor/preferences';
import { Capacitor } from '@capacitor/core';

const STORAGE_KEYS = {
  ACCESS_TOKEN: 'lyke_access_token',
  REFRESH_TOKEN: 'lyke_refresh_token',
  TOKEN_EXPIRY: 'lyke_token_expiry',
  USER_ID: 'lyke_user_id',
} as const;

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  private readonly platformId = inject(PLATFORM_ID);
  private readonly isNative = Capacitor.isNativePlatform();
  private readonly isBrowser = isPlatformBrowser(this.platformId);

  async set(key: string, value: string): Promise<void> {
    if (this.isNative) {
      await Preferences.set({ key, value });
    } else if (this.isBrowser) {
      localStorage.setItem(key, value);
    }
  }

  async get(key: string): Promise<string | null> {
    if (this.isNative) {
      const result = await Preferences.get({ key });
      return result.value;
    } else if (this.isBrowser) {
      return localStorage.getItem(key);
    }
    return null;
  }

  async remove(key: string): Promise<void> {
    if (this.isNative) {
      await Preferences.remove({ key });
    } else if (this.isBrowser) {
      localStorage.removeItem(key);
    }
  }

  async clear(): Promise<void> {
    if (this.isNative) {
      await Preferences.clear();
    } else if (this.isBrowser) {
      localStorage.clear();
    }
  }

  // Token-specific methods
  async setAccessToken(token: string): Promise<void> {
    await this.set(STORAGE_KEYS.ACCESS_TOKEN, token);
  }

  async getAccessToken(): Promise<string | null> {
    return this.get(STORAGE_KEYS.ACCESS_TOKEN);
  }

  async setRefreshToken(token: string): Promise<void> {
    await this.set(STORAGE_KEYS.REFRESH_TOKEN, token);
  }

  async getRefreshToken(): Promise<string | null> {
    return this.get(STORAGE_KEYS.REFRESH_TOKEN);
  }

  async setTokenExpiry(expiry: string): Promise<void> {
    await this.set(STORAGE_KEYS.TOKEN_EXPIRY, expiry);
  }

  async getTokenExpiry(): Promise<string | null> {
    return this.get(STORAGE_KEYS.TOKEN_EXPIRY);
  }

  async setUserId(userId: string): Promise<void> {
    await this.set(STORAGE_KEYS.USER_ID, userId);
  }

  async getUserId(): Promise<string | null> {
    return this.get(STORAGE_KEYS.USER_ID);
  }

  async clearAuthData(): Promise<void> {
    await Promise.all([
      this.remove(STORAGE_KEYS.ACCESS_TOKEN),
      this.remove(STORAGE_KEYS.REFRESH_TOKEN),
      this.remove(STORAGE_KEYS.TOKEN_EXPIRY),
      this.remove(STORAGE_KEYS.USER_ID),
    ]);
  }
}
