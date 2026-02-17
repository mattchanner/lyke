import { ErrorHandler, Injectable } from '@angular/core';
import { Router, NavigationEnd } from '@angular/router';
import { ApplicationInsights } from '@microsoft/applicationinsights-web';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class AppInsightsService {
  private appInsights: ApplicationInsights | null = null;

  constructor(private router: Router) {}

  initialize(): void {
    const connectionString = environment.appInsightsConnectionString;
    if (!connectionString) {
      return;
    }

    this.appInsights = new ApplicationInsights({
      config: {
        connectionString,
        enableAutoRouteTracking: false,
        disableFetchTracking: false,
        disableAjaxTracking: false,
        enableCorsCorrelation: true,
        enableRequestHeaderTracking: true,
        enableResponseHeaderTracking: true,
      },
    });

    this.appInsights.loadAppInsights();

    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.trackPageView(event.urlAfterRedirects);
      }
    });
  }

  trackPageView(uri: string): void {
    this.appInsights?.trackPageView({ uri });
  }

  trackException(error: Error, properties?: Record<string, string>): void {
    this.appInsights?.trackException({ exception: error }, properties);
  }

  trackEvent(name: string, properties?: Record<string, string>): void {
    this.appInsights?.trackEvent({ name }, properties);
  }

  setAuthenticatedUser(userId: string): void {
    this.appInsights?.setAuthenticatedUserContext(userId);
  }

  clearAuthenticatedUser(): void {
    this.appInsights?.clearAuthenticatedUserContext();
  }
}

@Injectable()
export class AppInsightsErrorHandler implements ErrorHandler {
  constructor(private appInsights: AppInsightsService) {}

  handleError(error: unknown): void {
    const err = error instanceof Error ? error : new Error(String(error));
    this.appInsights.trackException(err);
    console.error(err);
  }
}
