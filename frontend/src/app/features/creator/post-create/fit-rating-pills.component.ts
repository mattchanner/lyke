import { Component, input, output } from '@angular/core';
import { IonChip, IonLabel } from '@ionic/angular/standalone';
import { FitRating } from '../../../models';

interface RatingOption {
  value: FitRating;
  label: string;
  color: string;
}

@Component({
  selector: 'app-fit-rating-pills',
  standalone: true,
  imports: [IonChip, IonLabel],
  template: `
    <div class="pills">
      @for (opt of options; track opt.value) {
        <ion-chip
          [color]="selected() === opt.value ? opt.color : 'medium'"
          [outline]="selected() !== opt.value"
          (click)="ratingChanged.emit(opt.value)"
        >
          <ion-label>{{ opt.label }}</ion-label>
        </ion-chip>
      }
    </div>
  `,
  styles: [`
    .pills { display: flex; flex-wrap: wrap; gap: 6px; }
    ion-chip { --border-radius: 20px; font-size: 0.75rem; }
  `],
})
export class FitRatingPillsComponent {
  readonly selected = input<FitRating>();
  readonly ratingChanged = output<FitRating>();

  readonly options: RatingOption[] = [
    { value: FitRating.TooSmall, label: 'Too Small', color: 'danger' },
    { value: FitRating.SlightlySmall, label: 'Slightly Small', color: 'warning' },
    { value: FitRating.TrueToSize, label: 'True to Size', color: 'success' },
    { value: FitRating.SlightlyLarge, label: 'Slightly Large', color: 'warning' },
    { value: FitRating.TooLarge, label: 'Too Large', color: 'danger' },
  ];
}
