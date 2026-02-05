import { Injectable, inject } from '@angular/core';
import { ToastController } from '@ionic/angular/standalone';

export type ToastPosition = 'top' | 'bottom' | 'middle';
export type ToastColor =
  | 'primary'
  | 'secondary'
  | 'tertiary'
  | 'success'
  | 'warning'
  | 'danger'
  | 'light'
  | 'medium'
  | 'dark';

export interface ToastOptions {
  message: string;
  duration?: number;
  position?: ToastPosition;
  color?: ToastColor;
  icon?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ToastService {
  private readonly toastController = inject(ToastController);

  async show(options: ToastOptions): Promise<void> {
    const toast = await this.toastController.create({
      message: options.message,
      duration: options.duration ?? 3000,
      position: options.position ?? 'bottom',
      color: options.color,
      icon: options.icon,
      buttons: [
        {
          icon: 'close',
          role: 'cancel',
        },
      ],
    });

    await toast.present();
  }

  async success(message: string, duration = 3000): Promise<void> {
    await this.show({
      message,
      duration,
      color: 'success',
      icon: 'checkmark-circle',
    });
  }

  async error(message: string, duration = 4000): Promise<void> {
    await this.show({
      message,
      duration,
      color: 'danger',
      icon: 'alert-circle',
    });
  }

  async warning(message: string, duration = 3500): Promise<void> {
    await this.show({
      message,
      duration,
      color: 'warning',
      icon: 'warning',
    });
  }

  async info(message: string, duration = 3000): Promise<void> {
    await this.show({
      message,
      duration,
      color: 'primary',
      icon: 'information-circle',
    });
  }
}
