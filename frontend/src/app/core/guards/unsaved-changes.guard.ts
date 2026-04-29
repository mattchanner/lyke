import { inject } from '@angular/core';
import { CanDeactivateFn } from '@angular/router';
import { AlertController } from '@ionic/angular/standalone';

// Components that own a form and want the unsaved-changes prompt should
// implement this interface. The guard checks `hasUnsavedChanges()`; when true
// it offers Save draft / Discard / Stay. "Save draft" runs `saveAsDraft()` and
// allows navigation; the save runs in the background and the component's own
// success handler navigates to its list page.
export interface HasUnsavedChanges {
  hasUnsavedChanges(): boolean;
  saveAsDraft(): void;
}

export const unsavedChangesGuard: CanDeactivateFn<HasUnsavedChanges> = async (
  component,
) => {
  if (!component.hasUnsavedChanges()) {
    return true;
  }

  const alertCtrl = inject(AlertController);
  const alert = await alertCtrl.create({
    header: 'Unsaved changes',
    message: "You have unsaved work on this post. What would you like to do?",
    buttons: [
      { text: 'Stay', role: 'cancel' },
      {
        text: 'Save draft',
        role: 'save-draft',
        handler: () => {
          component.saveAsDraft();
        },
      },
      { text: 'Discard', role: 'destructive' },
    ],
  });

  await alert.present();
  const { role } = await alert.onDidDismiss();

  return role !== 'cancel' && role !== 'backdrop';
};
