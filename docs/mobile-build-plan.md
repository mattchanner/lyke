# LYKE Mobile App Build Plan

A comprehensive guide for building, signing, and distributing the LYKE mobile app for iOS and Android.

**Last Updated:** 2026-02-27

---

## Table of Contents

1. [Overview](#overview)
2. [Hardware Requirements](#hardware-requirements)
3. [Software Prerequisites](#software-prerequisites)
4. [Android Development](#android-development)
5. [iOS Development](#ios-development)
6. [App Signing & Certificates](#app-signing--certificates)
7. [Building for Distribution](#building-for-distribution)
8. [Store Submission](#store-submission)
9. [CI/CD Pipeline](#cicd-pipeline)
10. [Testing on Devices](#testing-on-devices)
11. [Troubleshooting](#troubleshooting)

---

## Overview

### What We're Building

LYKE is built using:
- **Ionic 8** - UI framework for hybrid mobile apps
- **Angular 20** - Frontend framework
- **Capacitor 8** - Native runtime that wraps the web app in a native container

Capacitor compiles the Angular/Ionic web app into native iOS and Android applications. The same codebase produces both platforms.

### Current Project Status

| Platform | Status | Notes |
|----------|--------|-------|
| Android | Partially configured | Platform added, debug builds working in CI |
| iOS | Not started | Platform not yet added |

### Key Files

```
frontend/
├── capacitor.config.ts      # Capacitor configuration
├── package.json             # Dependencies including @capacitor/android
├── android/                 # Android native project (auto-generated)
│   ├── app/
│   │   ├── build.gradle     # Android build configuration
│   │   └── src/main/        # Native Android code
│   └── gradlew              # Gradle wrapper
└── ios/                     # iOS native project (not yet created)
    └── App/
        └── App.xcodeproj    # Xcode project
```

---

## Hardware Requirements

### For Android Development

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| Operating System | Windows 10, macOS 10.14+, Linux | Windows 11, macOS 13+, Ubuntu 22.04 |
| RAM | 8 GB | 16 GB+ |
| Storage | 20 GB free | 50 GB+ SSD |
| CPU | Any modern x64 | Multi-core for faster builds |

**Android development works on Windows, macOS, or Linux.**

### For iOS Development

| Requirement | Minimum | Recommended |
|-------------|---------|-------------|
| Operating System | **macOS 13 (Ventura)** | macOS 14 (Sonoma) or later |
| Hardware | Any Mac with Apple Silicon or Intel | Mac with Apple Silicon (M1/M2/M3) |
| RAM | 8 GB | 16 GB+ |
| Storage | 50 GB free | 100 GB+ SSD |
| Xcode | 15.0+ | Latest stable version |

**iOS development requires a Mac.** There is no way to build iOS apps on Windows or Linux. Options if you don't have a Mac:

1. **Mac Mini** (~$599) - Most cost-effective dedicated build machine
2. **MacBook Air M2/M3** (~$999-$1199) - Portable development option
3. **Cloud Mac services** - Rent a Mac in the cloud:
   - [MacStadium](https://www.macstadium.com/) - From ~$79/month
   - [AWS EC2 Mac](https://aws.amazon.com/ec2/instance-types/mac/) - On-demand Mac instances
   - [GitHub Actions macOS runners](https://docs.github.com/en/actions/using-github-hosted-runners) - For CI/CD only

### For Physical Device Testing

| Device | Purpose | Recommended |
|--------|---------|-------------|
| Android phone | Testing on real hardware | Any device running Android 10+ |
| iPhone | Testing on real hardware | iPhone 11 or newer (iOS 15+) |
| USB-C/Lightning cable | Connecting devices | Quality cable for reliable debugging |

---

## Software Prerequisites

### All Platforms

Install these regardless of which platform you're building:

#### 1. Node.js (v22 LTS)

```bash
# Windows (using winget)
winget install OpenJS.NodeJS.LTS

# macOS (using Homebrew)
brew install node@22

# Verify installation
node --version  # Should show v22.x.x
npm --version   # Should show 10.x.x
```

#### 2. Ionic CLI & Capacitor CLI

```bash
npm install -g @ionic/cli
npm install -g @capacitor/cli

# Verify
ionic --version
cap --version
```

#### 3. Project Dependencies

```bash
cd frontend
npm install
```

### Android-Specific Software

#### 1. Java Development Kit (JDK 21)

```bash
# Windows (using winget)
winget install EclipseAdoptium.Temurin.21.JDK

# macOS (using Homebrew)
brew install openjdk@21

# Verify
java --version  # Should show 21.x.x
```

#### 2. Android Studio

Download from: https://developer.android.com/studio

**Installation steps:**
1. Download Android Studio (Hedgehog or newer)
2. Run installer, accept defaults
3. On first launch, complete the Setup Wizard
4. Install recommended SDK components when prompted

**Required SDK Components** (via SDK Manager in Android Studio):
- Android SDK Platform 34 (Android 14)
- Android SDK Build-Tools 34.0.0
- Android SDK Command-line Tools
- Android Emulator
- Android SDK Platform-Tools

#### 3. Environment Variables (Windows)

Add to System Environment Variables:

```
ANDROID_HOME = C:\Users\<username>\AppData\Local\Android\Sdk
JAVA_HOME = C:\Program Files\Eclipse Adoptium\jdk-21.x.x-hotspot
```

Add to PATH:
```
%ANDROID_HOME%\platform-tools
%ANDROID_HOME%\cmdline-tools\latest\bin
%ANDROID_HOME%\emulator
```

#### 4. Environment Variables (macOS/Linux)

Add to `~/.zshrc` or `~/.bashrc`:

```bash
export ANDROID_HOME=$HOME/Library/Android/sdk
export PATH=$PATH:$ANDROID_HOME/platform-tools
export PATH=$PATH:$ANDROID_HOME/cmdline-tools/latest/bin
export PATH=$PATH:$ANDROID_HOME/emulator
```

Then reload: `source ~/.zshrc`

### iOS-Specific Software (macOS Only)

#### 1. Xcode

```bash
# Install from Mac App Store (preferred)
# Or via command line:
xcode-select --install

# After Xcode installation, accept license:
sudo xcodebuild -license accept

# Install iOS simulators (via Xcode > Settings > Platforms)
```

#### 2. CocoaPods

```bash
# Install Ruby (if not present)
brew install ruby

# Install CocoaPods
sudo gem install cocoapods

# Verify
pod --version
```

#### 3. Add iOS Platform to Project

```bash
cd frontend
npm install @capacitor/ios
npx cap add ios
```

---

## Android Development

### Initial Setup (One-Time)

The Android platform is already added to this project. If starting fresh:

```bash
cd frontend
npm install @capacitor/android
npx cap add android
```

### Development Workflow

#### Step 1: Build the Web App

```bash
cd frontend
npm run build
# Or for production:
npx ng build --configuration production
```

#### Step 2: Sync to Native Project

```bash
npx cap sync android
```

This copies the built web assets to the Android project and updates native dependencies.

#### Step 3: Open in Android Studio

```bash
npx cap open android
```

This opens the `frontend/android` folder in Android Studio.

#### Step 4: Run on Emulator or Device

In Android Studio:
1. Select a device/emulator from the dropdown (top toolbar)
2. Click the green "Run" button (or press Shift+F10)

**Or via command line:**

```bash
# List available devices
adb devices

# Run on connected device
npx cap run android

# Run on specific device
npx cap run android --target <device-id>
```

### Creating an Android Emulator

1. Open Android Studio
2. Go to **Tools > Device Manager**
3. Click **Create Device**
4. Select a phone (e.g., Pixel 7)
5. Select a system image (e.g., API 34 with Google Play)
6. Finish and launch the emulator

### Live Reload (Hot Reload)

For faster development with instant updates:

```bash
# Terminal 1: Start Angular dev server
cd frontend
ionic serve

# Terminal 2: Run with live reload
npx cap run android --livereload --external
```

The app will reload automatically when you save changes.

### Building Debug APK

```bash
cd frontend

# Build web app
npm run build

# Sync to Android
npx cap sync android

# Build debug APK
cd android
./gradlew assembleDebug  # macOS/Linux
gradlew.bat assembleDebug  # Windows

# APK location:
# android/app/build/outputs/apk/debug/app-debug.apk
```

---

## iOS Development

### Initial Setup (One-Time, macOS Only)

```bash
cd frontend

# Install iOS package
npm install @capacitor/ios

# Add iOS platform
npx cap add ios

# Install CocoaPods dependencies
cd ios/App
pod install
cd ../..
```

### Development Workflow

#### Step 1: Build the Web App

```bash
cd frontend
npm run build
```

#### Step 2: Sync to Native Project

```bash
npx cap sync ios
```

#### Step 3: Open in Xcode

```bash
npx cap open ios
```

This opens `frontend/ios/App/App.xcworkspace` in Xcode.

#### Step 4: Run on Simulator or Device

In Xcode:
1. Select a simulator or connected device from the scheme dropdown
2. Click the "Run" button (or press Cmd+R)

**Or via command line:**

```bash
npx cap run ios

# Run on specific simulator
npx cap run ios --target "iPhone 15 Pro"
```

### Live Reload (Hot Reload)

```bash
# Terminal 1: Start dev server
ionic serve

# Terminal 2: Run with live reload
npx cap run ios --livereload --external
```

### Building for Simulator (Debug)

```bash
cd frontend/ios/App

# Build for simulator
xcodebuild -workspace App.xcworkspace \
  -scheme App \
  -configuration Debug \
  -destination 'platform=iOS Simulator,name=iPhone 15 Pro' \
  build
```

---

## App Signing & Certificates

### Android Signing

#### Debug Signing (Automatic)

Debug builds are automatically signed with a debug keystore. This is fine for development and testing but **cannot be uploaded to Google Play**.

#### Release Signing (Required for Play Store)

##### Step 1: Generate a Keystore

```bash
keytool -genkey -v \
  -keystore lyke-release.keystore \
  -alias lyke \
  -keyalg RSA \
  -keysize 2048 \
  -validity 10000
```

You'll be prompted for:
- Keystore password (remember this!)
- Key password
- Name, organization, location info

**CRITICAL: Back up this keystore file and passwords securely. If lost, you cannot update your app on Google Play.**

##### Step 2: Configure Gradle for Release Signing

Create `frontend/android/keystore.properties` (DO NOT commit to git):

```properties
storeFile=../lyke-release.keystore
storePassword=your_keystore_password
keyAlias=lyke
keyPassword=your_key_password
```

Add to `.gitignore`:
```
android/keystore.properties
android/*.keystore
```

##### Step 3: Update build.gradle

Edit `frontend/android/app/build.gradle`:

```gradle
android {
    // ... existing config ...

    signingConfigs {
        release {
            def keystorePropertiesFile = rootProject.file("keystore.properties")
            if (keystorePropertiesFile.exists()) {
                def keystoreProperties = new Properties()
                keystoreProperties.load(new FileInputStream(keystorePropertiesFile))
                storeFile file(keystoreProperties['storeFile'])
                storePassword keystoreProperties['storePassword']
                keyAlias keystoreProperties['keyAlias']
                keyPassword keystoreProperties['keyPassword']
            }
        }
    }

    buildTypes {
        release {
            signingConfig signingConfigs.release
            minifyEnabled true
            proguardFiles getDefaultProguardFile('proguard-android-optimize.txt'), 'proguard-rules.pro'
        }
    }
}
```

### iOS Signing

iOS signing is more complex and requires an Apple Developer account ($99/year).

#### Step 1: Enroll in Apple Developer Program

1. Go to https://developer.apple.com/programs/enroll/
2. Sign in with your Apple ID
3. Complete enrollment ($99/year)
4. Wait for approval (usually 24-48 hours)

#### Step 2: Create App ID

1. Go to https://developer.apple.com/account/resources/identifiers/list
2. Click "+" to register a new identifier
3. Select "App IDs" > "App"
4. Enter description: "LYKE App"
5. Bundle ID: `clothing.belyke.app` (must match capacitor.config.ts)
6. Select capabilities needed (Push Notifications, Sign in with Apple, etc.)
7. Register

#### Step 3: Create Certificates

**Development Certificate** (for testing on devices):

1. Open Keychain Access on your Mac
2. Go to Keychain Access > Certificate Assistant > Request a Certificate from a Certificate Authority
3. Enter your email, select "Saved to disk"
4. Go to https://developer.apple.com/account/resources/certificates/list
5. Click "+" > "Apple Development"
6. Upload the certificate request file
7. Download and double-click to install

**Distribution Certificate** (for App Store):

Same process but select "Apple Distribution" certificate type.

#### Step 4: Create Provisioning Profiles

**Development Profile:**

1. Go to https://developer.apple.com/account/resources/profiles/list
2. Click "+" > "iOS App Development"
3. Select your App ID
4. Select your development certificate
5. Select devices for testing
6. Name it "LYKE Development"
7. Download and double-click to install

**Distribution Profile:**

Same process but select "App Store Connect" distribution.

#### Step 5: Configure Xcode

1. Open project in Xcode: `npx cap open ios`
2. Select the "App" target
3. Go to "Signing & Capabilities" tab
4. Enable "Automatically manage signing"
5. Select your team (your Apple Developer account)
6. Xcode will create/download certificates automatically

---

## Building for Distribution

### Android: Release APK/AAB

```bash
cd frontend

# Build production web app
npx ng build --configuration production

# Sync to Android
npx cap sync android

# Build release APK
cd android
./gradlew assembleRelease

# Or build Android App Bundle (preferred for Play Store)
./gradlew bundleRelease

# Output locations:
# APK: android/app/build/outputs/apk/release/app-release.apk
# AAB: android/app/build/outputs/bundle/release/app-release.aab
```

### iOS: Archive for App Store

1. Open Xcode: `npx cap open ios`
2. Select "Any iOS Device" as the build target
3. Go to **Product > Archive**
4. Wait for archive to complete
5. Organizer window opens automatically
6. Click "Distribute App"
7. Select "App Store Connect"
8. Follow the wizard to upload

**Or via command line:**

```bash
cd frontend/ios/App

# Archive
xcodebuild -workspace App.xcworkspace \
  -scheme App \
  -configuration Release \
  -archivePath build/App.xcarchive \
  archive

# Export for App Store
xcodebuild -exportArchive \
  -archivePath build/App.xcarchive \
  -exportPath build/AppStore \
  -exportOptionsPlist ExportOptions.plist
```

---

## Store Submission

### Google Play Console Setup

#### Step 1: Create Developer Account

1. Go to https://play.google.com/console/signup
2. Pay one-time $25 registration fee
3. Complete account details and verification

#### Step 2: Create App Listing

1. Click "Create app"
2. Enter app name: "LYKE"
3. Select default language, app/game, free/paid
4. Accept policies

#### Step 3: Complete Store Listing

Required assets:
- **App icon**: 512x512 PNG
- **Feature graphic**: 1024x500 PNG
- **Screenshots**: Min 2, max 8 per device type
  - Phone: 16:9 or 9:16 aspect ratio
  - 7-inch tablet (optional)
  - 10-inch tablet (optional)
- **Short description**: Max 80 characters
- **Full description**: Max 4000 characters
- **Privacy policy URL**: Required

#### Step 4: Upload App Bundle

1. Go to "Release" > "Production"
2. Click "Create new release"
3. Upload your `.aab` file
4. Add release notes
5. Review and roll out

#### Step 5: Content Rating

Complete the content rating questionnaire (required before publishing).

#### Step 6: App Review

Google reviews typically take 1-3 days. First submission may take longer.

### App Store Connect Setup

#### Step 1: Create App Record

1. Go to https://appstoreconnect.apple.com
2. Click "My Apps" > "+"
3. Enter app name: "LYKE"
4. Select bundle ID: `clothing.belyke.app`
5. Select primary language

#### Step 2: Complete App Information

Required information:
- **App icon**: 1024x1024 PNG (no alpha/transparency)
- **Screenshots**: Required for each device size
  - 6.7" (iPhone 15 Pro Max): 1290x2796 or 2796x1290
  - 6.5" (iPhone 14 Plus): 1284x2778 or 2778x1284
  - 5.5" (iPhone 8 Plus): 1242x2208 or 2208x1242
  - iPad Pro 12.9" (optional): 2048x2732
- **Description**: Up to 4000 characters
- **Keywords**: Up to 100 characters
- **Support URL**: Required
- **Privacy policy URL**: Required

#### Step 3: Upload Build

After archiving in Xcode and uploading to App Store Connect:

1. Go to your app in App Store Connect
2. Click "+" next to Build
3. Select the uploaded build
4. Add "What's New" text

#### Step 4: App Review

Submit for review. Apple review typically takes 1-2 days but can vary. First submissions often get more scrutiny.

**Common rejection reasons:**
- Crashes or bugs
- Broken links
- Placeholder content
- Privacy policy issues
- Guideline violations

---

## CI/CD Pipeline

### Current Setup

The project already has a GitHub Actions workflow for Android debug builds at `.github/workflows/build-android.yml`.

### Enhanced Android Pipeline (Release Builds)

Create `.github/workflows/build-android-release.yml`:

```yaml
name: Build Android Release

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

jobs:
  build:
    runs-on: ubuntu-latest

    steps:
      - uses: actions/checkout@v4

      - name: Set up Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'
          cache: 'npm'
          cache-dependency-path: frontend/package-lock.json

      - name: Set up JDK 21
        uses: actions/setup-java@v4
        with:
          distribution: 'temurin'
          java-version: '21'

      - name: Install dependencies
        working-directory: frontend
        run: npm ci

      - name: Build production app
        working-directory: frontend
        run: npx ng build --configuration production

      - name: Sync Capacitor
        working-directory: frontend
        run: npx cap sync android

      - name: Decode keystore
        run: |
          echo "${{ secrets.ANDROID_KEYSTORE_BASE64 }}" | base64 -d > frontend/android/lyke-release.keystore

      - name: Create keystore.properties
        run: |
          cat > frontend/android/keystore.properties << EOF
          storeFile=lyke-release.keystore
          storePassword=${{ secrets.KEYSTORE_PASSWORD }}
          keyAlias=${{ secrets.KEY_ALIAS }}
          keyPassword=${{ secrets.KEY_PASSWORD }}
          EOF

      - name: Build release AAB
        working-directory: frontend/android
        run: |
          chmod +x gradlew
          ./gradlew bundleRelease

      - name: Upload AAB
        uses: actions/upload-artifact@v4
        with:
          name: app-release
          path: frontend/android/app/build/outputs/bundle/release/app-release.aab
```

**Required GitHub Secrets:**
- `ANDROID_KEYSTORE_BASE64`: Base64-encoded keystore file
- `KEYSTORE_PASSWORD`: Keystore password
- `KEY_ALIAS`: Key alias (e.g., "lyke")
- `KEY_PASSWORD`: Key password

To encode keystore:
```bash
base64 -i lyke-release.keystore -o keystore-base64.txt
```

### iOS Pipeline (Requires macOS Runner)

Create `.github/workflows/build-ios.yml`:

```yaml
name: Build iOS

on:
  push:
    tags:
      - 'v*'
  workflow_dispatch:

jobs:
  build:
    runs-on: macos-14  # macOS Sonoma with Xcode 15

    steps:
      - uses: actions/checkout@v4

      - name: Set up Node.js
        uses: actions/setup-node@v4
        with:
          node-version: '22'

      - name: Install dependencies
        working-directory: frontend
        run: npm ci

      - name: Build production app
        working-directory: frontend
        run: npx ng build --configuration production

      - name: Add iOS platform
        working-directory: frontend
        run: |
          npm install @capacitor/ios
          npx cap add ios

      - name: Sync Capacitor
        working-directory: frontend
        run: npx cap sync ios

      - name: Install CocoaPods
        run: |
          cd frontend/ios/App
          pod install

      - name: Install Apple certificate
        uses: apple-actions/import-codesign-certs@v2
        with:
          p12-file-base64: ${{ secrets.APPLE_CERTIFICATE_P12 }}
          p12-password: ${{ secrets.APPLE_CERTIFICATE_PASSWORD }}

      - name: Install provisioning profile
        uses: apple-actions/download-provisioning-profiles@v2
        with:
          bundle-id: clothing.belyke.app
          issuer-id: ${{ secrets.APPSTORE_ISSUER_ID }}
          api-key-id: ${{ secrets.APPSTORE_KEY_ID }}
          api-private-key: ${{ secrets.APPSTORE_PRIVATE_KEY }}

      - name: Build archive
        working-directory: frontend/ios/App
        run: |
          xcodebuild -workspace App.xcworkspace \
            -scheme App \
            -configuration Release \
            -archivePath $PWD/build/App.xcarchive \
            archive

      - name: Export IPA
        working-directory: frontend/ios/App
        run: |
          xcodebuild -exportArchive \
            -archivePath $PWD/build/App.xcarchive \
            -exportPath $PWD/build/IPA \
            -exportOptionsPlist ExportOptions.plist

      - name: Upload IPA
        uses: actions/upload-artifact@v4
        with:
          name: app-release-ios
          path: frontend/ios/App/build/IPA/*.ipa
```

---

## Testing on Devices

### Android Device Testing

#### Enable Developer Options

1. Go to **Settings > About phone**
2. Tap "Build number" 7 times
3. Go back to **Settings > Developer options**
4. Enable "USB debugging"

#### Connect and Run

```bash
# Connect device via USB
# Verify device is detected
adb devices

# Run app on device
cd frontend
npx cap run android --target <device-id>
```

#### Wireless Debugging (Android 11+)

```bash
# Enable wireless debugging on device
# In Developer options > Wireless debugging

# Pair device
adb pair <device-ip>:<pairing-port>

# Connect
adb connect <device-ip>:<port>

# Now run as normal
npx cap run android
```

### iOS Device Testing

#### Register Device

1. Connect iPhone to Mac
2. Open Xcode
3. Go to **Window > Devices and Simulators**
4. Your device appears; note the UDID
5. Add UDID to your provisioning profile at developer.apple.com

#### Run on Device

```bash
cd frontend
npx cap run ios --target <device-name>
```

Or in Xcode, select your device and click Run.

### TestFlight (iOS Beta Testing)

1. Archive app in Xcode
2. Upload to App Store Connect
3. Go to TestFlight section
4. Add internal testers (up to 100, instant access)
5. Add external testers (up to 10,000, requires review)
6. Testers download TestFlight app and install your build

### Google Play Internal Testing

1. Go to Play Console > Internal testing
2. Create a new release
3. Upload AAB
4. Add testers by email
5. Testers opt-in via link and download from Play Store

---

## Troubleshooting

### Common Android Issues

#### "SDK location not found"

Create `frontend/android/local.properties`:
```properties
sdk.dir=C:\\Users\\<username>\\AppData\\Local\\Android\\Sdk
```
(Use forward slashes on macOS/Linux)

#### "Gradle build failed"

```bash
cd frontend/android
./gradlew clean
./gradlew assembleDebug --stacktrace
```

#### "INSTALL_FAILED_UPDATE_INCOMPATIBLE"

Uninstall the existing app from the device:
```bash
adb uninstall clothing.belyke.app
```

### Common iOS Issues

#### "No signing certificate"

1. Open Xcode
2. Go to Preferences > Accounts
3. Select your Apple ID
4. Click "Download Manual Profiles"

#### "Pod install failed"

```bash
cd frontend/ios/App
pod repo update
pod install --repo-update
```

#### "Code signing error"

1. In Xcode, go to Signing & Capabilities
2. Uncheck "Automatically manage signing"
3. Re-check it
4. Select your team again

### Common Capacitor Issues

#### "Capacitor sync failed"

```bash
# Clear and rebuild
cd frontend
rm -rf node_modules
rm -rf www
npm install
npm run build
npx cap sync
```

#### "Plugin not found in native project"

```bash
# Remove and re-add platform
npx cap rm android  # or ios
npx cap add android  # or ios
npx cap sync
```

---

## Checklist Summary

### Before First Build

- [ ] Node.js 22 installed
- [ ] Ionic CLI and Capacitor CLI installed
- [ ] Project dependencies installed (`npm install`)

### Android Setup

- [ ] JDK 21 installed
- [ ] Android Studio installed
- [ ] Android SDK components installed
- [ ] Environment variables configured (ANDROID_HOME, JAVA_HOME)
- [ ] Android platform added to project

### iOS Setup (macOS only)

- [ ] Xcode installed and license accepted
- [ ] CocoaPods installed
- [ ] iOS platform added to project (`npx cap add ios`)
- [ ] Apple Developer account enrolled ($99/year)
- [ ] Certificates and provisioning profiles created

### For Release

- [ ] Android keystore created and backed up
- [ ] iOS distribution certificate created
- [ ] App icons and screenshots prepared
- [ ] Privacy policy published
- [ ] Store listings completed
- [ ] CI/CD secrets configured

---

## Quick Reference Commands

```bash
# Build and sync
npm run build && npx cap sync

# Open in IDE
npx cap open android
npx cap open ios

# Run on device/emulator
npx cap run android
npx cap run ios

# Live reload development
ionic serve
npx cap run android --livereload --external

# Build release APK
cd android && ./gradlew assembleRelease

# Build release AAB
cd android && ./gradlew bundleRelease
```
