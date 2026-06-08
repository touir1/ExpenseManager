#!/usr/bin/env node
/**
 * Installs and launches the debug APK on a connected Android device or emulator.
 * Reads appId from package.json — no hardcoded values.
 * APK is located by glob inside android/app/build/outputs/apk/debug/ so the
 * exact filename (default: app-debug.apk) does not need to be hardcoded.
 *
 * Usage: node scripts/android-deploy.js [target]
 *   target: 'device'   → adb -d  (USB device, default)
 *           'emulator' → adb -e  (running emulator)
 */
const { execSync } = require('child_process');
const path = require('path');
const fs = require('fs');

const { appId } = require('../package.json');
const target = process.argv[2] || 'device';
const adbFlag = target === 'emulator' ? '-e' : '-d';

const apkDir = path.join(__dirname, '..', 'android', 'app', 'build', 'outputs', 'apk', 'debug');
const apkFiles = fs.readdirSync(apkDir).filter(f => f.endsWith('.apk'));
if (apkFiles.length === 0) {
  console.error(`No APK found in ${apkDir}`);
  process.exit(1);
}
// Prefer the first found; assembleDebug produces exactly one file
const apkPath = path.join(apkDir, apkFiles[0]);
console.log(`Installing ${apkFiles[0]}...`);

const run = cmd => execSync(cmd, { stdio: 'inherit', shell: true });

try {
  run(`adb ${adbFlag} install -r "${apkPath}"`);
  run(`adb ${adbFlag} shell monkey -p ${appId} -c android.intent.category.LAUNCHER 1`);
} catch (e) {
  process.exit(e.status ?? 1);
}
