import { Component, inject, signal, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonButton,
  IonInput,
  IonItem,
  IonSpinner,
  IonText,
  IonLabel,
  IonIcon,
  IonSegment,
  IonSegmentButton,
  ModalController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { checkmarkOutline } from 'ionicons/icons';
import { ApiService, ToastService, ProfileStateService } from '../../../core';
import {
  BodyProfileResponse,
  BodyTypeResponse,
  FrameSizeResponse,
  UpdateBodyProfileRequest,
  FitPreference,
} from '../../../models';
import { QuizResponse } from '../../../models/quiz/quiz.model';
import { QuizPage } from '../../quiz/quiz.page';

@Component({
  selector: 'app-body-profile-edit',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonButton,
    IonInput,
    IonItem,
    IonSpinner,
    IonText,
    IonLabel,
    IonIcon,
    IonSegment,
    IonSegmentButton,
  ],
  templateUrl: './body-profile-edit.page.html',
  styleUrls: ['./body-profile-edit.page.scss'],
})
export class BodyProfileEditPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly profileState = inject(ProfileStateService);
  private readonly modalCtrl = inject(ModalController);

  readonly FitPreference = FitPreference;

  readonly isLoading = signal(false);
  readonly isSaving = signal(false);
  readonly heightUnit = signal<'cm' | 'ft'>('cm');
  readonly weightUnit = signal<'kg' | 'lbs'>('kg');
  readonly bodyTypes = signal<BodyTypeResponse[]>([]);
  readonly frameSizes = signal<FrameSizeResponse[]>([]);
  readonly selectedBodyTypeId = signal<number | null>(null);
  readonly selectedFrameSizeId = signal<number | null>(null);
  readonly selectedFitPreferences = signal<Set<FitPreference>>(new Set());

  readonly heightCmControl = this.fb.control<number | null>(null, [
    Validators.required,
    Validators.min(100),
    Validators.max(250),
  ]);
  readonly heightFtControl = this.fb.control<number | null>(null, [
    Validators.min(3),
    Validators.max(8),
  ]);
  readonly heightInControl = this.fb.control<number | null>(null, [
    Validators.min(0),
    Validators.max(11),
  ]);
  readonly weightControl = this.fb.control<number | null>(null, [
    Validators.required,
    Validators.min(30),
  ]);

  private originalProfile: BodyProfileResponse | null = null;

  constructor() {
    addIcons({ checkmarkOutline });
  }

  ngOnInit(): void {
    this.isLoading.set(true);
    this.loadBodyTypes();
    this.loadFrameSizes();
    this.loadProfile();
  }

  loadBodyTypes(): void {
    this.api.get<BodyTypeResponse[]>('lookup', 'body-types').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.bodyTypes.set(response.data);
        }
      },
    });
  }

  loadFrameSizes(): void {
    this.api.get<FrameSizeResponse[]>('lookup', 'frame-sizes').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.frameSizes.set(response.data);
        }
      },
    });
  }

  loadProfile(): void {
    this.api.get<BodyProfileResponse>('profile', 'body').subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.originalProfile = response.data;
          this.populateForm(response.data);
        }
      },
      complete: () => this.isLoading.set(false),
    });
  }

  private populateForm(profile: BodyProfileResponse): void {
    this.heightCmControl.setValue(profile.heightCm);
    this.weightControl.setValue(profile.weightKg);
    this.selectedBodyTypeId.set(profile.bodyTypeId);
    this.selectedFrameSizeId.set(profile.frameSizeId);
    this.selectedFitPreferences.set(new Set(profile.fitPreferences));
  }

  onHeightUnitChange(event: CustomEvent): void {
    const newUnit = event.detail.value as 'cm' | 'ft';
    if (newUnit === 'ft' && this.heightCmControl.value) {
      const totalInches = this.heightCmControl.value / 2.54;
      this.heightFtControl.setValue(Math.floor(totalInches / 12));
      this.heightInControl.setValue(Math.round(totalInches % 12));
    } else if (newUnit === 'cm' && this.heightFtControl.value !== null) {
      this.heightCmControl.setValue(this.getHeightCm());
    }
    this.heightUnit.set(newUnit);
  }

  onWeightUnitChange(event: CustomEvent): void {
    const newUnit = event.detail.value as 'kg' | 'lbs';
    if (newUnit === 'lbs' && this.weightControl.value) {
      this.weightControl.setValue(Math.round(this.weightControl.value / 0.453592));
    } else if (newUnit === 'kg' && this.weightControl.value) {
      this.weightControl.setValue(Math.round(this.weightControl.value * 0.453592 * 10) / 10);
    }
    this.weightUnit.set(newUnit);
  }

  selectBodyType(id: number): void {
    this.selectedBodyTypeId.set(id);
  }

  selectFrameSize(id: number): void {
    this.selectedFrameSizeId.set(id);
  }

  toggleFitPreference(preference: FitPreference): void {
    this.selectedFitPreferences.update((current) => {
      const next = new Set(current);
      if (next.has(preference)) {
        next.delete(preference);
      } else {
        next.add(preference);
      }
      return next;
    });
  }

  isFitPreferenceSelected(preference: FitPreference): boolean {
    return this.selectedFitPreferences().has(preference);
  }

  getHeightCm(): number {
    if (this.heightUnit() === 'cm') {
      return this.heightCmControl.value!;
    }
    const feet = this.heightFtControl.value || 0;
    const inches = this.heightInControl.value || 0;
    return Math.round((feet * 12 + inches) * 2.54);
  }

  getWeightKg(): number {
    const weight = this.weightControl.value!;
    if (this.weightUnit() === 'kg') {
      return weight;
    }
    return Math.round(weight * 0.453592 * 10) / 10;
  }

  get isFormValid(): boolean {
    const heightValid =
      this.heightUnit() === 'cm'
        ? this.heightCmControl.valid
        : this.heightFtControl.value !== null && this.heightInControl.value !== null;
    return (
      heightValid &&
      this.weightControl.valid &&
      this.selectedBodyTypeId() !== null
    );
  }

  get hasChanges(): boolean {
    if (!this.originalProfile) return false;

    const fitPrefsChanged = !this.areSetsEqual(
      this.selectedFitPreferences(),
      new Set(this.originalProfile.fitPreferences),
    );

    return (
      this.getHeightCm() !== this.originalProfile.heightCm ||
      this.getWeightKg() !== this.originalProfile.weightKg ||
      this.selectedBodyTypeId() !== this.originalProfile.bodyTypeId ||
      this.selectedFrameSizeId() !== this.originalProfile.frameSizeId ||
      fitPrefsChanged
    );
  }

  private areSetsEqual(a: Set<FitPreference>, b: Set<FitPreference>): boolean {
    if (a.size !== b.size) return false;
    for (const item of a) {
      if (!b.has(item)) return false;
    }
    return true;
  }

  async openQuiz(): Promise<void> {
    const modal = await this.modalCtrl.create({
      component: QuizPage,
      componentProps: { isEmbedded: true },
    });
    await modal.present();
    const { data, role } = await modal.onWillDismiss<QuizResponse>();
    if (role === 'apply' && data) {
      this.selectedBodyTypeId.set(data.bodyTypeId);
      this.toast.success(`Body type set to ${data.resultLabel}. You can still change it below.`);
    }
  }

  onSubmit(): void {
    if (!this.isFormValid || !this.hasChanges || this.isSaving()) return;

    this.isSaving.set(true);

    const request: UpdateBodyProfileRequest = {};
    const orig = this.originalProfile!;

    const heightCm = this.getHeightCm();
    if (heightCm !== orig.heightCm) request.heightCm = heightCm;

    const weightKg = this.getWeightKg();
    if (weightKg !== orig.weightKg) request.weightKg = weightKg;

    const bodyTypeId = this.selectedBodyTypeId()!;
    if (bodyTypeId !== orig.bodyTypeId) request.bodyTypeId = bodyTypeId;

    const frameSizeId = this.selectedFrameSizeId();
    if (frameSizeId !== orig.frameSizeId) request.frameSizeId = frameSizeId;

    const fitPrefsChanged = !this.areSetsEqual(
      this.selectedFitPreferences(),
      new Set(orig.fitPreferences),
    );
    if (fitPrefsChanged) {
      request.fitPreferences = Array.from(this.selectedFitPreferences());
    }

    this.api.put('profile', 'body', request).subscribe({
      next: (response) => {
        if (response.success) {
          // Optimistically update the cached body profile from form values
          const heightCmVal = this.getHeightCm();
          const weightKgVal = this.getWeightKg();
          const bodyTypeIdVal = this.selectedBodyTypeId()!;
          const frameSizeIdVal = this.selectedFrameSizeId();
          const bodyTypeName = this.bodyTypes().find(bt => bt.id === bodyTypeIdVal)?.name ?? orig.bodyTypeName;
          const frameSizeName = frameSizeIdVal != null
            ? (this.frameSizes().find(fs => fs.id === frameSizeIdVal)?.name ?? orig.frameSizeName)
            : null;
          this.profileState.setBodyProfile({
            ...orig,
            heightCm: heightCmVal,
            weightKg: weightKgVal,
            heightDisplay: `${heightCmVal} cm`,
            weightDisplay: `${weightKgVal} kg`,
            bodyTypeId: bodyTypeIdVal,
            bodyTypeName,
            frameSizeId: frameSizeIdVal,
            frameSizeName,
            fitPreferences: Array.from(this.selectedFitPreferences()),
            updatedAt: new Date().toISOString(),
          });
          this.toast.success('Body profile updated');
          this.router.navigate(['/profile']);
        } else {
          this.toast.error(response.error?.message || 'Failed to update body profile');
        }
      },
      error: () => {
        this.toast.error('Failed to update body profile');
        this.isSaving.set(false);
      },
      complete: () => this.isSaving.set(false),
    });
  }
}
