import { Component, inject, signal, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
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
  IonLabel,
  IonSelect,
  IonSelectOption,
  IonSegment,
  IonSegmentButton,
  IonProgressBar,
  IonAccordionGroup,
  IonAccordion,
  IonAvatar,
  ModalController,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  arrowForwardOutline,
  checkmarkOutline,
  helpCircleOutline,
  cameraOutline,
  imageOutline,
  personAddOutline,
} from 'ionicons/icons';
import { ApiService, ToastService, AnalyticsService, AuthService } from '../../core';
import {
  BodyTypeResponse,
  FrameSizeResponse,
  CreateBodyProfileRequest,
  FitPreference,
} from '../../models';
import { QuizResponse } from '../../models/quiz/quiz.model';
import { QuizPage } from '../quiz/quiz.page';

@Component({
  selector: 'app-onboarding',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
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
    IonLabel,
    IonSelect,
    IonSelectOption,
    IonSegment,
    IonSegmentButton,
    IonProgressBar,
    IonAccordionGroup,
    IonAccordion,
    IonAvatar,
  ],
  templateUrl: './onboarding.page.html',
  styleUrls: ['./onboarding.page.scss'],
})
export class OnboardingPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly analytics = inject(AnalyticsService);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);
  private readonly modalCtrl = inject(ModalController);

  readonly FitPreference = FitPreference;

  readonly currentStep = signal(1);
  readonly totalSteps = 6;
  readonly isLoading = signal(false);
  readonly photoPreview = signal<string | null>(null);
  readonly isUploading = signal(false);

  readonly heightUnit = signal<'cm' | 'ft'>('cm');
  readonly weightUnit = signal<'kg' | 'lbs'>('kg');

  readonly bodyTypes = signal<BodyTypeResponse[]>([]);
  readonly frameSizes = signal<FrameSizeResponse[]>([]);
  readonly selectedBodyTypeId = signal<number | null>(null);
  readonly selectedFrameSizeId = signal<number | null>(null);
  readonly selectedFitPreferences = signal<Set<FitPreference>>(new Set());
  readonly selectedStature = signal<string | null>(null);
  readonly selectedBuild = signal<string | null>(null);

  // Form controls
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

  constructor() {
    addIcons({ arrowForwardOutline, checkmarkOutline, helpCircleOutline, cameraOutline, imageOutline, personAddOutline });
  }

  ngOnInit(): void {
    this.loadBodyTypes();
    this.loadFrameSizes();
  }

  progress(): number {
    return this.currentStep() / this.totalSteps;
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

  onHeightUnitChange(event: CustomEvent): void {
    this.heightUnit.set(event.detail.value);
  }

  onWeightUnitChange(event: CustomEvent): void {
    this.weightUnit.set(event.detail.value);
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

  bodyShapeIcon(name: string): string {
    const slug = name.toLowerCase().replace(/\s+/g, '-');
    return `assets/body-shapes/${slug}.svg`;
  }

  canProceed(): boolean {
    switch (this.currentStep()) {
      case 1:
        if (this.heightUnit() === 'cm') {
          return this.heightCmControl.valid;
        } else {
          return (
            this.heightFtControl.value !== null &&
            this.heightInControl.value !== null
          );
        }
      case 2:
        return this.weightControl.valid;
      case 3:
        return this.selectedBodyTypeId() !== null;
      case 4:
        return this.selectedFrameSizeId() !== null;
      case 5:
        return this.selectedFitPreferences().size > 0;
      default:
        return true;
    }
  }

  canSubmit(): boolean {
    return this.selectedFitPreferences().size > 0;
  }

  nextStep(): void {
    if (this.canProceed() && this.currentStep() < this.totalSteps) {
      this.currentStep.update((s) => s + 1);
    }
  }

  previousStep(): void {
    if (this.currentStep() > 1) {
      this.currentStep.update((s) => s - 1);
    }
  }

  getHeightCm(): number {
    if (this.heightUnit() === 'cm') {
      return this.heightCmControl.value!;
    } else {
      const feet = this.heightFtControl.value || 0;
      const inches = this.heightInControl.value || 0;
      return Math.round((feet * 12 + inches) * 2.54);
    }
  }

  getWeightKg(): number {
    const weight = this.weightControl.value!;
    if (this.weightUnit() === 'kg') {
      return weight;
    } else {
      return Math.round(weight * 0.453592 * 10) / 10;
    }
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
      this.selectedStature.set(data.stature);
      this.selectedBuild.set(data.build);

      const frameSizeName = data.build === 'Plus' ? 'Plus' : data.stature;
      const matchedFrameSize = this.frameSizes().find((fs) => fs.name === frameSizeName);
      if (matchedFrameSize) {
        this.selectedFrameSizeId.set(matchedFrameSize.id);
      }

      this.toast.success(`Body type set to ${data.resultLabel}. You can still change it below.`);
    }
  }

  onSubmit(): void {
    if (this.selectedFitPreferences().size === 0) return;

    this.isLoading.set(true);

    const request: CreateBodyProfileRequest = {
      heightCm: this.getHeightCm(),
      weightKg: this.getWeightKg(),
      bodyTypeId: this.selectedBodyTypeId()!,
      frameSizeId: this.selectedFrameSizeId(),
      fitPreferences: Array.from(this.selectedFitPreferences()),
      stature: this.selectedStature() ?? undefined,
      build: this.selectedBuild() ?? undefined,
    };

    this.api.post('profile', 'body', request).subscribe({
      next: (response) => {
        if (response.success) {
          this.analytics.track('profile.complete', {
            bodyTypeId: String(this.selectedBodyTypeId()),
            frameSizeId: String(this.selectedFrameSizeId()),
            fitPreferences: Array.from(this.selectedFitPreferences()).join(','),
          });
          this.toast.success('Profile complete!');
          this.currentStep.set(6);
        }
      },
      error: () => {
        this.isLoading.set(false);
      },
      complete: () => {
        this.isLoading.set(false);
      },
    });
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    if (file.size > 5 * 1024 * 1024) {
      this.toast.error('Image must be 5MB or smaller');
      input.value = '';
      return;
    }
    // Preview
    const reader = new FileReader();
    reader.onload = () => this.photoPreview.set(reader.result as string);
    reader.readAsDataURL(file);
    // Upload
    this.isUploading.set(true);
    const formData = new FormData();
    formData.append('file', file, file.name);
    this.api.uploadFile<any>('profile', 'me/image', formData).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.auth.setProfileImageUrl(response.data.profileImageUrl);
          this.toast.success('Photo uploaded!');
        }
      },
      error: () => {
        this.toast.error('Failed to upload photo');
        this.isUploading.set(false);
      },
      complete: () => this.isUploading.set(false),
    });
  }

  skipPhoto(): void {
    localStorage.setItem('photoSkippedAt', new Date().toISOString());
    this.router.navigate(['/feed']);
  }

  finishOnboarding(): void {
    this.router.navigate(['/feed']);
  }
}
