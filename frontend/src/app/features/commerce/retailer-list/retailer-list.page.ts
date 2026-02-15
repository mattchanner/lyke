import { Component, inject, signal, OnInit } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  IonContent,
  IonHeader,
  IonTitle,
  IonToolbar,
  IonBackButton,
  IonButtons,
  IonSpinner,
  IonGrid,
  IonRow,
  IonCol,
  IonCard,
  IonCardContent,
} from '@ionic/angular/standalone';
import { CommerceService } from '../../../core/services/commerce.service';
import { RetailerResponse } from '../../../models';

@Component({
  selector: 'app-retailer-list',
  standalone: true,
  imports: [
    RouterLink,
    IonContent,
    IonHeader,
    IonTitle,
    IonToolbar,
    IonBackButton,
    IonButtons,
    IonSpinner,
    IonGrid,
    IonRow,
    IonCol,
    IonCard,
    IonCardContent,
  ],
  templateUrl: './retailer-list.page.html',
  styleUrls: ['./retailer-list.page.scss'],
})
export class RetailerListPage implements OnInit {
  private readonly commerce = inject(CommerceService);

  readonly retailers = signal<RetailerResponse[]>([]);
  readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadRetailers();
  }

  loadRetailers(): void {
    this.isLoading.set(true);
    this.commerce.getRetailers().subscribe({
      next: (retailers) => this.retailers.set(retailers),
      complete: () => this.isLoading.set(false),
    });
  }

  getInitial(name: string): string {
    return name.charAt(0).toUpperCase();
  }
}
