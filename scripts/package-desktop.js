import { execSync } from 'child_process';
import fs from 'fs';
import path from 'path';
import { fileURLToPath } from 'url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const rootDir = path.resolve(__dirname, '..');

console.log('=== Rashtra: Packaging Engine-Free Desktop Release ===\n');

// 1. Build Web Production Bundle
console.log('1. Building web production bundle (Vite + TypeScript)...');
execSync('npm run build', { cwd: rootDir, stdio: 'inherit' });

// 2. Publish Lightweight Windows WebView2 Executable
const releaseDir = path.join(rootDir, 'release', 'Rashtra-Desktop-x64');
if (fs.existsSync(releaseDir)) {
  fs.rmSync(releaseDir, { recursive: true, force: true });
}
fs.mkdirSync(releaseDir, { recursive: true });

console.log('\n2. Compiling native Windows desktop runner (Rashtra.exe)...');
const csprojPath = path.join(rootDir, 'desktop', 'Rashtra.Desktop', 'Rashtra.Desktop.csproj');
execSync(
  `dotnet publish "${csprojPath}" -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o "${releaseDir}"`,
  { cwd: rootDir, stdio: 'inherit' }
);

// 3. Copy dist-web into release bundle
console.log('\n3. Embedding web assets into release bundle...');
const distWebSource = path.join(rootDir, 'dist-web');
const distWebTarget = path.join(releaseDir, 'dist-web');

function copyDirRecursive(src, dest) {
  fs.mkdirSync(dest, { recursive: true });
  for (const entry of fs.readdirSync(src, { withFileTypes: true })) {
    const srcPath = path.join(src, entry.name);
    const destPath = path.join(dest, entry.name);
    if (entry.isDirectory()) {
      copyDirRecursive(srcPath, destPath);
    } else {
      fs.copyFileSync(srcPath, destPath);
    }
  }
}
copyDirRecursive(distWebSource, distWebTarget);

// 4. Create README-PLAY.txt and Quick Launchers
const readmeContent = `=====================================================
Rashtra: Sovereign Republic - Windows Desktop Release
=====================================================

HOW TO PLAY:
1. Double-click "Rashtra.exe" to launch the standalone desktop game.
2. No game engine, Godot, or command prompt installation required!
3. Microsoft Edge WebView2 runtime (built into Windows 10 & 11) is used for rendering.

KEYBOARD SHORTCUTS:
- Space       : Pause / Resume simulation clock
- 1, 2, 3     : Simulation speed (Normal, Fast, Ultra)
- F1          : Cabinet & Ideology panel
- F2          : Presidential Tasks panel
- F3          : Domestic Events & Intelligence wire
- F4          : Economy & National Budget panel
- F5          : QuickSave
- F6          : Diplomacy & Foreign Trade panel
- F7          : Armed Forces & Military Operations
- F8          : National Achievements
- F9          : QuickLoad / Save Manager
- Esc         : Close active panel / modal

Enjoy leading your sovereign republic!
`;
fs.writeFileSync(path.join(releaseDir, 'README-PLAY.txt'), readmeContent, 'utf-8');

// Also create Launch-Offline.bat
const batContent = `@echo off
start "" "%~dp0Rashtra.exe"
`;
fs.writeFileSync(path.join(releaseDir, 'Launch-Offline.bat'), batContent, 'utf-8');

// Calculate total size
function getFolderSize(dirPath) {
  let total = 0;
  for (const file of fs.readdirSync(dirPath, { withFileTypes: true })) {
    const fullPath = path.join(dirPath, file.name);
    if (file.isDirectory()) {
      total += getFolderSize(fullPath);
    } else {
      total += fs.statSync(fullPath).size;
    }
  }
  return total;
}

const totalBytes = getFolderSize(releaseDir);
const totalMB = (totalBytes / (1024 * 1024)).toFixed(2);
const exeBytes = fs.statSync(path.join(releaseDir, 'Rashtra.exe')).size;
const exeMB = (exeBytes / (1024 * 1024)).toFixed(2);

console.log('\n=====================================================');
console.log(`Packaging Complete!`);
console.log(`Output Directory: release/Rashtra-Desktop-x64`);
console.log(`Executable Size (Rashtra.exe): ${exeMB} MB`);
console.log(`Total Release Bundle Size: ${totalMB} MB (previously 133 MB in Godot!)`);
console.log('=====================================================\n');
