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
  template: `
    <ion-header>
      <ion-toolbar>
        <ion-title>Complete Your Profile</ion-title>
      </ion-toolbar>
      <ion-progress-bar [value]="progress()"></ion-progress-bar>
    </ion-header>

    <ion-content class="ion-padding">
      <div class="onboarding-container">
        @switch (currentStep()) {
          @case (1) {
            <div class="step-content">
              <h2>What's your height?</h2>
              <p>This helps us show you outfits from people with similar builds.</p>

              <div class="unit-toggle">
                <ion-segment [value]="heightUnit()" (ionChange)="onHeightUnitChange($event)">
                  <ion-segment-button value="cm">
                    <ion-label>cm</ion-label>
                  </ion-segment-button>
                  <ion-segment-button value="ft">
                    <ion-label>ft/in</ion-label>
                  </ion-segment-button>
                </ion-segment>
              </div>

              @if (heightUnit() === 'cm') {
                <ion-item>
                  <ion-input
                    type="number"
                    [formControl]="heightCmControl"
                    placeholder="Height in cm"
                    min="100"
                    max="250"
                  ></ion-input>
                  <ion-text slot="end">cm</ion-text>
                </ion-item>
              } @else {
                <div class="feet-inches">
                  <ion-item>
                    <ion-input
                      type="number"
                      [formControl]="heightFtControl"
                      placeholder="Feet"
                      min="3"
                      max="8"
                    ></ion-input>
                    <ion-text slot="end">ft</ion-text>
                  </ion-item>
                  <ion-item>
                    <ion-input
                      type="number"
                      [formControl]="heightInControl"
                      placeholder="Inches"
                      min="0"
                      max="11"
                    ></ion-input>
                    <ion-text slot="end">in</ion-text>
                  </ion-item>
                </div>
              }
            </div>
          }

          @case (2) {
            <div class="step-content">
              <h2>What's your weight?</h2>
              <p>This is kept private and only used to find similar body profiles.</p>

              <div class="unit-toggle">
                <ion-segment [value]="weightUnit()" (ionChange)="onWeightUnitChange($event)">
                  <ion-segment-button value="kg">
                    <ion-label>kg</ion-label>
                  </ion-segment-button>
                  <ion-segment-button value="lbs">
                    <ion-label>lbs</ion-label>
                  </ion-segment-button>
                </ion-segment>
              </div>

              <ion-item>
                <ion-input
                  type="number"
                  [formControl]="weightControl"
                  [placeholder]="'Weight in ' + weightUnit()"
                  min="30"
                  [max]="weightUnit() === 'kg' ? 300 : 660"
                ></ion-input>
                <ion-text slot="end">{{ weightUnit() }}</ion-text>
              </ion-item>
            </div>
          }

          @case (3) {
            <div class="step-content">
              <h2>What's your body type?</h2>
              <p>Select the option that best describes your shape.</p>

              <div class="body-type-grid">
                @for (bodyType of bodyTypes(); track bodyType.id) {
                  <div
                    class="body-type-card"
                    [class.selected]="selectedBodyTypeId() === bodyType.id"
                    (click)="selectBodyType(bodyType.id)"
                  >
                    <span class="body-type-name">{{ bodyType.name }}</span>
                    @if (bodyType.description) {
                      <span class="body-type-desc">{{ bodyType.description }}</span>
                    }
                  </div>
                }
              </div>
            </div>
          }

          @case (4) {
            <div class="step-content">
              <h2>How do you like your clothes to fit?</h2>
              <p>This helps us recommend the right sizes for you.</p>

              <div class="fit-preference-grid">
                <div
                  class="fit-card"
                  [class.selected]="selectedFitPreference() === FitPreference.Fitted"
                  (click)="selectFitPreference(FitPreference.Fitted)"
                >
                  <span class="fit-name">Fitted</span>
                  <span class="fit-desc">Close to the body</span>
                </div>
                <div
                  class="fit-card"
                  [class.selected]="selectedFitPreference() === FitPreference.Regular"
                  (click)="selectFitPreference(FitPreference.Regular)"
                >
                  <span class="fit-name">Regular</span>
                  <span class="fit-desc">Standard fit</span>
                </div>
                <div
                  class="fit-card"
                  [class.selected]="selectedFitPreference() === FitPreference.Relaxed"
                  (click)="selectFitPreference(FitPreference.Relaxed)"
                >
                  <span class="fit-name">Relaxed</span>
                  <span class="fit-desc">Loose and comfortable</span>
                </div>
              </div>
            </div>
          }
        }

        <div class="navigation-buttons">
          @if (currentStep() > 1) {
            <ion-button fill="outline" (click)="previousStep()">
              Back
            </ion-button>
          }

          @if (currentStep() < 4) {
            <ion-button
              [disabled]="!canProceed()"
              (click)="nextStep()"
            >
              Next
              <ion-icon name="arrow-forward-outline" slot="end"></ion-icon>
            </ion-button>
          } @else {
            <ion-button
              [disabled]="!canSubmit() || isLoading()"
              (click)="onSubmit()"
            >
              @if (isLoading()) {
                <ion-spinner name="crescent"></ion-spinner>
              } @else {
                Complete
                <ion-icon name="checkmark-outline" slot="end"></ion-icon>
              }
            </ion-button>
          }
        </div>
      </div>
    </ion-content>
  `,
  styles: [`
    .onboarding-container {
      max-width: 500px;
      margin: 0 auto;
      min-height: 100%;
      display: flex;
      flex-direction: column;
    }

    .step-content {
      flex: 1;
      padding-top: 2rem;

      h2 {
        font-size: 1.5rem;
        font-weight: 700;
        margin-bottom: 0.5rem;
      }

      p {
        color: var(--ion-color-medium);
        margin-bottom: 2rem;
      }
    }

    .unit-toggle {
      margin-bottom: 1.5rem;

      ion-segment {
        max-width: 200px;
      }
    }

    ion-item {
      --background: var(--ion-color-light);
      --border-radius: 8px;
      margin-bottom: 0.5rem;
    }

    .feet-inches {
      display: flex;
      gap: 1rem;

      ion-item {
        flex: 1;
      }
    }

    .body-type-grid {
      display: grid;
      grid-template-columns: repeat(2, 1fr);
      gap: 1rem;
    }

    .body-type-card {
      padding: 1.25rem;
      border: 2px solid var(--ion-color-light);
      border-radius: 12px;
      text-align: center;
      cursor: pointer;
      transition: all 0.2s ease;

      &.selected {
        border-color: var(--ion-color-primary);
        background: var(--ion-color-primary-tint);
      }

      .body-type-name {
        display: block;
        font-weight: 600;
        margin-bottom: 0.25rem;
      }

      .body-type-desc {
        display: block;
        font-size: 0.85rem;
        color: var(--ion-color-medium);
      }
    }

    .fit-preference-grid {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .fit-card {
      padding: 1.25rem;
      border: 2px solid var(--ion-color-light);
      border-radius: 12px;
      cursor: pointer;
      transition: all 0.2s ease;

      &.selected {
        border-color: var(--ion-color-primary);
        background: var(--ion-color-primary-tint);
      }

      .fit-name {
        display: block;
        font-weight: 600;
        margin-bottom: 0.25rem;
      }

      .fit-desc {
        display: block;
        font-size: 0.9rem;
        color: var(--ion-color-medium);
      }
    }

    .navigation-buttons {
      display: flex;
      justify-content: space-between;
      gap: 1rem;
      padding: 1.5rem 0;

      ion-button {
        --border-radius: 8px;
        flex: 1;
      }
    }
  `],
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
    this.api.get<BodyTypeResponse[]>('profile', 'body-types').subscribe({
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

    this.api.post('profile', 'body-profile', request).subscribe({
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
