#!/usr/bin/env node
/**
 * Runs a Gradle task inside the android/ directory with JDK 21.
 * Works on Windows (cmd/PowerShell) and bash — avoids bash-only JAVA_HOME=... syntax.
 *
 * Usage: node scripts/android-gradle.js [task]
 *   task defaults to 'assembleDebug'
 *   e.g. node scripts/android-gradle.js bundleRelease
 */
const { execSync } = require('child_process');
const path = require('path');

const JAVA_HOME = 'C:/Program Files/Microsoft/jdk-21.0.10.7-hotspot';
const androidDir = path.join(__dirname, '..', 'android');
const isWin = process.platform === 'win32';
// On Windows, gradlew.bat must be called via its full absolute path
const gradlew = isWin ? path.join(androidDir, 'gradlew.bat') : './gradlew';
const task = process.argv[2] || 'assembleDebug';

try {
  execSync(`"${gradlew}" ${task}`, {
    cwd: androidDir,
    env: { ...process.env, JAVA_HOME },
    stdio: 'inherit',
    shell: true,
  });
} catch (e) {
  process.exit(e.status ?? 1);
}
