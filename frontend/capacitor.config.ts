import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'clothing.belyke.app',
  appName: 'Be-Lyke',
  webDir: 'www',
  plugins: {
    SocialLogin: {
      google: {
        webClientId: 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com',
      },
    },
  },
};

export default config;
