import type { CapacitorConfig } from '@capacitor/cli';

const config: CapacitorConfig = {
  appId: 'clothing.belyke.app',
  appName: 'LYKE',
  webDir: 'www',
  server: {
    androidScheme: 'http',
  },
  plugins: {
    SocialLogin: {
      google: {
        webClientId: 'YOUR_GOOGLE_CLIENT_ID.apps.googleusercontent.com',
      },
    },
  },
};

export default config;
