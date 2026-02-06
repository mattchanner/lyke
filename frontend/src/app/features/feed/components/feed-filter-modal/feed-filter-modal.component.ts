import { Component, inject, input, output, signal, computed, OnInit } from '@angular/core';
import {
  IonModal,
  IonHeader,
  IonToolbar,
  IonTitle,
  IonButtons,
  IonButton,
  IonIcon,
  IonContent,
  IonChip,
  IonLabel,
  IonFooter,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import { closeOutline, refreshOutline } from 'ionicons/icons';
import { ApiService } from '../../../../core';
import { CommerceService } from '../../../../core/services/commerce.service';
import { FitTagResponse, RetailerResponse } from '../../../../models';

export interface FeedFilters {
  category: string | null;
  retailerId: string | null;
  fitTagIds: number[];
}

interface FitTagGroup {
  category: string;
  tags: FitTagResponse[];
}

@Component({
  selector: 'app-feed-filter-modal',
  standalone: true,
  imports: [
    IonModal,
    IonHeader,
    IonToolbar,
    IonTitle,
    IonButtons,
    IonButton,
    IonIcon,
    IonContent,
    IonChip,
    IonLabel,
    IonFooter,
  ],
  templateUrl: './feed-filter-modal.component.html',
  styleUrls: ['./feed-filter-modal.component.scss'],
})
export class FeedFilterModalComponent implements OnInit {
  private readonly api = inject(ApiService);
  private readonly commerce = inject(CommerceService);

  readonly isOpen = input(false);
  readonly currentFilters = input<FeedFilters>({ category: null, retailerId: null, fitTagIds: [] });

  readonly filtersChanged = output<FeedFilters>();
  readonly dismissed = output<void>();

  readonly categories = ['Tops', 'Bottoms', 'Dresses', 'Outerwear', 'Shoes', 'Accessories'];

  readonly retailers = signal<RetailerResponse[]>([]);
  readonly fitTagGroups = signal<FitTagGroup[]>([]);

  // Local selection state
  readonly selectedCategory = signal<string | null>(null);
  readonly selectedRetailerId = signal<string | null>(null);
  readonly selectedFitTagIds = signal<number[]>([]);

  readonly activeCount = computed(() => {
    let count = 0;
    if (this.selectedCategory()) count++;
    if (this.selectedRetailerId()) count++;
    count += this.selectedFitTagIds().length;
    return count;
  });

  constructor() {
    addIcons({ closeOutline, refreshOutline });
  }

  ngOnInit(): void {
    this.loadRetailers();
    this.loadFitTags();
  }

  onModalPresented(): void {
    const filters = this.currentFilters();
    this.selectedCategory.set(filters.category);
    this.selectedRetailerId.set(filters.retailerId);
    this.selectedFitTagIds.set([...filters.fitTagIds]);
  }

  toggleCategory(category: string): void {
    this.selectedCategory.set(
      this.selectedCategory() === category ? null : category
    );
  }

  toggleRetailer(retailerId: string): void {
    this.selectedRetailerId.set(
      this.selectedRetailerId() === retailerId ? null : retailerId
    );
  }

  toggleFitTag(tagId: number): void {
    this.selectedFitTagIds.update(ids =>
      ids.includes(tagId) ? ids.filter(id => id !== tagId) : [...ids, tagId]
    );
  }

  isFitTagSelected(tagId: number): boolean {
    return this.selectedFitTagIds().includes(tagId);
  }

  reset(): void {
    this.selectedCategory.set(null);
    this.selectedRetailerId.set(null);
    this.selectedFitTagIds.set([]);
  }

  apply(): void {
    this.filtersChanged.emit({
      category: this.selectedCategory(),
      retailerId: this.selectedRetailerId(),
      fitTagIds: this.selectedFitTagIds(),
    });
    this.dismissed.emit();
  }

  close(): void {
    this.dismissed.emit();
  }

  private loadRetailers(): void {
    this.commerce.getRetailers().subscribe(retailers => {
      this.retailers.set(retailers);
    });
  }

  private loadFitTags(): void {
    this.api.get<FitTagResponse[]>('lookup', 'fit-tags').subscribe(response => {
      if (response.success && response.data) {
        const grouped = new Map<string, FitTagResponse[]>();
        for (const tag of response.data) {
          const cat = tag.category ?? 'General';
          const existing = grouped.get(cat) ?? [];
          existing.push(tag);
          grouped.set(cat, existing);
        }
        this.fitTagGroups.set(
          Array.from(grouped.entries()).map(([category, tags]) => ({ category, tags }))
        );
      }
    });
  }
}
