import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IonAvatar,
  IonContent,
  IonHeader,
  IonIcon,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonInput,
  IonItem,
  IonSpinner,
  IonText,
  ModalController,
} from '@ionic/angular/standalone';
import { ApiService, AuthService, ToastService, ProfileStateService } from '../../../core';
import {
  UserProfileResponse,
  UpdateProfileRequest,
} from '../../../models';
import { ImageCropModalComponent } from './image-crop-modal.component';

@Component({
  selector: 'app-profile-edit',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    IonAvatar,
    IonContent,
    IonHeader,
    IonIcon,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonInput,
    IonItem,
    IonSpinner,
    IonText,
  ],
  templateUrl: './profile-edit.page.html',
  styleUrls: ['./profile-edit.page.scss'],
})
export class ProfileEditPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly modalCtrl = inject(ModalController);
  private readonly profileState = inject(ProfileStateService);

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly isUploading = signal(false);
  readonly imagePreview = signal<string | null>(null);

  readonly form = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
  });

  private originalEmail = '';

  ngOnInit(): void {
    this.loadProfile();
  }

  get isDirty(): boolean {
    return this.form.value.email !== this.originalEmail;
  }

  loadProfile(): void {
    this.isLoading.set(true);
    this.api.get<UserProfileResponse>('profile', 'me').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.originalEmail = response.data.email;
          this.form.patchValue({ email: response.data.email });
          this.imagePreview.set(response.data.profileImageUrl);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  onSubmit(): void {
    if (this.form.invalid || !this.isDirty || this.isSaving()) return;

    this.isSaving.set(true);
    const request: UpdateProfileRequest = {
      email: this.form.value.email!,
    };

    this.api.put<UserProfileResponse>('profile', '', request).subscribe({
      next: (response) => {
        if (response.success) {
          this.profileState.updateProfile({ email: request.email });
          this.toast.success('Profile updated');
          this.router.navigate(['/profile']);
        } else {
          this.toast.error(response.error?.message || 'Failed to update profile');
        }
      },
      error: () => {
        this.toast.error('Failed to update profile');
        this.isSaving.set(false);
      },
      complete: () => this.isSaving.set(false),
    });
  }

  async onFileSelected(event: Event): Promise<void> {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    if (file.size > 5 * 1024 * 1024) {
      this.toast.error('Image must be 5MB or smaller');
      input.value = '';
      return;
    }

    const modal = await this.modalCtrl.create({
      component: ImageCropModalComponent,
      componentProps: { imageFile: file },
    });
    await modal.present();

    const { data: croppedBlob } = await modal.onDidDismiss<Blob>();
    input.value = '';

    if (!croppedBlob) return;

    this.isUploading.set(true);
    const formData = new FormData();
    formData.append('file', croppedBlob, file.name);

    this.api.uploadFile<UserProfileResponse>('profile', 'me/image', formData).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.imagePreview.set(response.data.profileImageUrl);
          this.auth.setProfileImageUrl(response.data.profileImageUrl);
          this.profileState.updateProfile({ profileImageUrl: response.data.profileImageUrl });
          this.toast.success('Profile image updated');
        } else {
          this.toast.error(response.error?.message || 'Failed to upload image');
        }
      },
      error: () => {
        this.toast.error('Failed to upload image');
        this.isUploading.set(false);
      },
      complete: () => this.isUploading.set(false),
    });
  }
}
