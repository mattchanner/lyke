import { Injectable, signal } from '@angular/core';
import { UserProfileResponse, BodyProfileResponse } from '../../models';

@Injectable({ providedIn: 'root' })
export class ProfileStateService {
  private readonly _profile = signal<UserProfileResponse | null>(null);
  private readonly _bodyProfile = signal<BodyProfileResponse | null>(null);

  readonly profile = this._profile.asReadonly();
  readonly bodyProfile = this._bodyProfile.asReadonly();

  setProfile(profile: UserProfileResponse): void {
    this._profile.set(profile);
  }

  setBodyProfile(body: BodyProfileResponse): void {
    this._bodyProfile.set(body);
  }

  updateProfile(partial: Partial<UserProfileResponse>): void {
    const current = this._profile();
    if (current) {
      this._profile.set({ ...current, ...partial });
    }
  }

  updateBodyProfile(partial: Partial<BodyProfileResponse>): void {
    const current = this._bodyProfile();
    if (current) {
      this._bodyProfile.set({ ...current, ...partial });
    }
  }

  clear(): void {
    this._profile.set(null);
    this._bodyProfile.set(null);
  }
}
