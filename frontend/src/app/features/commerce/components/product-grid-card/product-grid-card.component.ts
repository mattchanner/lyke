import { Component, Input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { DecimalPipe } from '@angular/common';
import {
  IonCard,
  IonCardContent,
  IonBadge,
} from '@ionic/angular/standalone';
import { ProductResponse } from '../../../../models';

@Component({
  selector: 'app-product-grid-card',
  standalone: true,
  imports: [RouterLink, DecimalPipe, IonCard, IonCardContent, IonBadge],
  templateUrl: './product-grid-card.component.html',
  styleUrls: ['./product-grid-card.component.scss'],
})
export class ProductGridCardComponent {
  @Input({ required: true }) product!: ProductResponse;
}
