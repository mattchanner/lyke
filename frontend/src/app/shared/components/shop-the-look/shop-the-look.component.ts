import { Component, Input } from '@angular/core';
import { IonBadge } from '@ionic/angular/standalone';
import { PostProductDetailResponse } from '../../../models';
import { ProductCardComponent } from '../product-card';

@Component({
  selector: 'app-shop-the-look',
  standalone: true,
  imports: [IonBadge, ProductCardComponent],
  templateUrl: './shop-the-look.component.html',
  styleUrls: ['./shop-the-look.component.scss'],
})
export class ShopTheLookComponent {
  @Input({ required: true }) products!: PostProductDetailResponse[];
  @Input({ required: true }) postId!: string;
}
