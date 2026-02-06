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
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { arrowForwardOutline, checkmarkOutline } from 'ionicons/icons';
import { ApiService, ToastService } from '../../core';
import {
  BodyTypeResponse,
  CreateBodyProfileRequest,
  FitPreference,
} from '../../models';

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
  ],
  templateUrl: './onboarding.page.html',
  styleUrls: ['./onboarding.page.scss'],
})
export class OnboardingPage implements OnInit {
  private readonly fb = inject(FormBuilder);
  private readonly api = inject(ApiService);
  private readonly router = inject(Router);
  private readonly toast = inject(ToastService);

  readonly FitPreference = FitPreference;

  readonly currentStep = signal(1);
  readonly totalSteps = 4;
  readonly isLoading = signal(false);

  readonly heightUnit = signal<'cm' | 'ft'>('cm');
  readonly weightUnit = signal<'kg' | 'lbs'>('kg');

  readonly bodyTypes = signal<BodyTypeResponse[]>([]);
  readonly selectedBodyTypeId = signal<number | null>(null);
  readonly selectedFitPreference = signal<FitPreference | null>(null);

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
    addIcons({ arrowForwardOutline, checkmarkOutline });
  }

  ngOnInit(): void {
    this.loadBodyTypes();
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

  onHeightUnitChange(event: CustomEvent): void {
    this.heightUnit.set(event.detail.value);
  }

  onWeightUnitChange(event: CustomEvent): void {
    this.weightUnit.set(event.detail.value);
  }

  selectBodyType(id: number): void {
    this.selectedBodyTypeId.set(id);
  }

  selectFitPreference(preference: FitPreference): void {
    this.selectedFitPreference.set(preference);
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
      default:
        return true;
    }
  }

  canSubmit(): boolean {
    return this.selectedFitPreference() !== null;
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

  onSubmit(): void {
    if (!this.canSubmit()) return;

    this.isLoading.set(true);

    const request: CreateBodyProfileRequest = {
      heightCm: this.getHeightCm(),
      weightKg: this.getWeightKg(),
      bodyTypeId: this.selectedBodyTypeId()!,
      fitPreference: this.selectedFitPreference(),
    };

    this.api.post('profile', 'body', request).subscribe({
      next: (response) => {
        if (response.success) {
          this.toast.success('Profile complete!');
          this.router.navigate(['/feed']);
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
}
