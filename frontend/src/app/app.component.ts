import { Component, inject } from '@angular/core';
import { RouterLinkActive, RouterLink } from '@angular/router';
import {
  IonApp,
  IonRouterOutlet,
  IonSplitPane,
  IonMenu,
  IonContent,
  IonList,
  IonListHeader,
  IonItem,
  IonIcon,
  IonLabel,
  IonMenuToggle,
} from '@ionic/angular/standalone';
import { addIcons } from 'ionicons';
import {
  newspaperOutline,
  compassOutline,
  bookmarkOutline,
  bagOutline,
  gridOutline,
  imagesOutline,
  addCircleOutline,
  analyticsOutline,
  walletOutline,
  checkmarkCircleOutline,
  personOutline,
  cubeOutline,
  megaphoneOutline,
  bodyOutline,
  peopleOutline,
  personCircleOutline,
  settingsOutline,
  flagOutline,
  warningOutline,
  trendingUpOutline,
  heartOutline,
  pricetagsOutline
} from 'ionicons/icons';
import { AppInsightsService, AuthService, DeepLinkService, WebVitalsService } from './core';

@Component({
  selector: 'app-root',
  templateUrl: 'app.component.html',
  styleUrls: ['app.component.scss'],
  imports: [
    RouterLink,
    RouterLinkActive,
    IonApp,
    IonRouterOutlet,
    IonSplitPane,
    IonMenu,
    IonContent,
    IonList,
    IonListHeader,
    IonItem,
    IonIcon,
    IonLabel,
    IonMenuToggle,
  ],
})
export class AppComponent {
  readonly auth = inject(AuthService);

  constructor() {
    inject(AppInsightsService).initialize();
    inject(DeepLinkService).initialize();
    inject(WebVitalsService).initialize();

    addIcons({
      newspaperOutline,
      compassOutline,
      bookmarkOutline,
      bagOutline,
      gridOutline,
      imagesOutline,
      addCircleOutline,
      analyticsOutline,
      walletOutline,
      checkmarkCircleOutline,
      personOutline,
      cubeOutline,
      megaphoneOutline,
      bodyOutline,
      peopleOutline,
      personCircleOutline,
      settingsOutline,
      flagOutline,
      warningOutline,
      trendingUpOutline,
      heartOutline,
      pricetagsOutline
    });
  }
}
