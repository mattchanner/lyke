import { TestBed } from '@angular/core/testing';
import { ToastController } from '@ionic/angular/standalone';
import { ToastService } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;
  let toastControllerSpy: jasmine.SpyObj<ToastController>;
  let mockToast: jasmine.SpyObj<HTMLIonToastElement>;

  beforeEach(() => {
    mockToast = jasmine.createSpyObj('HTMLIonToastElement', ['present']);
    mockToast.present.and.returnValue(Promise.resolve());

    toastControllerSpy = jasmine.createSpyObj('ToastController', ['create']);
    toastControllerSpy.create.and.returnValue(Promise.resolve(mockToast));

    TestBed.configureTestingModule({
      providers: [{ provide: ToastController, useValue: toastControllerSpy }],
    });
    service = TestBed.inject(ToastService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should create and present a toast with show()', async () => {
    await service.show({ message: 'Test message', duration: 2000, color: 'primary' });

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'Test message',
        duration: 2000,
        color: 'primary',
      })
    );
    expect(mockToast.present).toHaveBeenCalled();
  });

  it('should use default duration and position in show()', async () => {
    await service.show({ message: 'Hello' });

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'Hello',
        duration: 3000,
        position: 'bottom',
      })
    );
  });

  it('should show success toast with correct color and icon', async () => {
    await service.success('Done!');

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'Done!',
        color: 'success',
        icon: 'checkmark-circle',
        duration: 3000,
      })
    );
  });

  it('should show error toast with correct color and icon', async () => {
    await service.error('Failed!');

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'Failed!',
        color: 'danger',
        icon: 'alert-circle',
        duration: 4000,
      })
    );
  });

  it('should show warning toast with correct color and icon', async () => {
    await service.warning('Careful!');

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'Careful!',
        color: 'warning',
        icon: 'warning',
        duration: 3500,
      })
    );
  });

  it('should show info toast with correct color and icon', async () => {
    await service.info('FYI');

    expect(toastControllerSpy.create).toHaveBeenCalledWith(
      jasmine.objectContaining({
        message: 'FYI',
        color: 'primary',
        icon: 'information-circle',
        duration: 3000,
      })
    );
  });
});
