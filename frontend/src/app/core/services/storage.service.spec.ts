import { TestBed } from '@angular/core/testing';
import { PLATFORM_ID } from '@angular/core';
import { StorageService } from './storage.service';

describe('StorageService', () => {
  let service: StorageService;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [{ provide: PLATFORM_ID, useValue: 'browser' }],
    });
    service = TestBed.inject(StorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // Basic CRUD
  it('should set and get a value', async () => {
    await service.set('testKey', 'testValue');
    const result = await service.get('testKey');
    expect(result).toBe('testValue');
  });

  it('should return null for missing key', async () => {
    const result = await service.get('nonexistent');
    expect(result).toBeNull();
  });

  it('should remove a value', async () => {
    await service.set('toRemove', 'val');
    await service.remove('toRemove');
    const result = await service.get('toRemove');
    expect(result).toBeNull();
  });

  it('should clear all values', async () => {
    await service.set('key1', 'val1');
    await service.set('key2', 'val2');
    await service.clear();
    expect(await service.get('key1')).toBeNull();
    expect(await service.get('key2')).toBeNull();
  });

  // Token helpers
  it('should set and get access token', async () => {
    await service.setAccessToken('access123');
    expect(await service.getAccessToken()).toBe('access123');
  });

  it('should set and get refresh token', async () => {
    await service.setRefreshToken('refresh456');
    expect(await service.getRefreshToken()).toBe('refresh456');
  });

  it('should set and get token expiry', async () => {
    const expiry = '2026-12-31T23:59:59Z';
    await service.setTokenExpiry(expiry);
    expect(await service.getTokenExpiry()).toBe(expiry);
  });

  it('should set and get user ID', async () => {
    await service.setUserId('user-abc');
    expect(await service.getUserId()).toBe('user-abc');
  });

  // clearAuthData
  it('should clear all auth data', async () => {
    await service.setAccessToken('t1');
    await service.setRefreshToken('t2');
    await service.setTokenExpiry('t3');
    await service.setUserId('t4');

    await service.clearAuthData();

    expect(await service.getAccessToken()).toBeNull();
    expect(await service.getRefreshToken()).toBeNull();
    expect(await service.getTokenExpiry()).toBeNull();
    expect(await service.getUserId()).toBeNull();
  });

  it('should not clear non-auth data when clearAuthData is called', async () => {
    await service.set('custom_key', 'custom_value');
    await service.setAccessToken('tok');

    await service.clearAuthData();

    expect(await service.get('custom_key')).toBe('custom_value');
  });

  it('should overwrite existing values', async () => {
    await service.set('key', 'original');
    await service.set('key', 'updated');
    expect(await service.get('key')).toBe('updated');
  });

  it('should return null for access token when not set', async () => {
    expect(await service.getAccessToken()).toBeNull();
  });

  it('should return null for refresh token when not set', async () => {
    expect(await service.getRefreshToken()).toBeNull();
  });
});
