import * as fs from 'fs';
import * as path from 'path';
import { ISaveStorage } from './SaveManager';

export class FileSaveStorage implements ISaveStorage {
  private baseDir: string;

  constructor(baseDir: string) {
    this.baseDir = baseDir;
    try {
      if (!fs.existsSync(this.baseDir)) {
        fs.mkdirSync(this.baseDir, { recursive: true });
      }
    } catch {
      // ignore
    }
  }

  private getPath(key: string): string {
    return path.join(this.baseDir, `${key}.json`);
  }

  getItem(key: string): string | null {
    const p = this.getPath(key);
    try {
      if (fs.existsSync(p)) {
        return fs.readFileSync(p, 'utf-8');
      }
    } catch {
      // ignore
    }
    return null;
  }

  setItem(key: string, value: string): void {
    const target = this.getPath(key);
    const temp = path.join(this.baseDir, `${key}.json.tmp`);
    const bak = path.join(this.baseDir, `${key}.json.bak`);

    // 1. Write to temp file
    fs.writeFileSync(temp, value, 'utf-8');

    // 2. Backup if target exists
    if (fs.existsSync(target)) {
      try {
        fs.copyFileSync(target, bak);
      } catch {
        // ignore
      }
    }

    // 3. Atomically rename/replace
    try {
      if (fs.existsSync(target)) {
        fs.unlinkSync(target);
      }
      fs.renameSync(temp, target);
    } catch {
      fs.writeFileSync(target, value, 'utf-8');
      try {
        if (fs.existsSync(temp)) fs.unlinkSync(temp);
      } catch {
        // ignore
      }
    }
  }

  removeItem(key: string): void {
    const target = this.getPath(key);
    const bak = path.join(this.baseDir, `${key}.json.bak`);
    try {
      if (fs.existsSync(target)) fs.unlinkSync(target);
      if (fs.existsSync(bak)) fs.unlinkSync(bak);
    } catch {
      // ignore
    }
  }

  hasItem(key: string): boolean {
    return fs.existsSync(this.getPath(key));
  }

  listKeys(): string[] {
    try {
      if (!fs.existsSync(this.baseDir)) return [];
      const files = fs.readdirSync(this.baseDir);
      return files
        .filter((f) => f.endsWith('.json') && !f.endsWith('.tmp') && !f.endsWith('.bak'))
        .map((f) => f.replace(/\.json$/, ''));
    } catch {
      return [];
    }
  }

  getSize(key: string): number {
    try {
      const p = this.getPath(key);
      if (fs.existsSync(p)) {
        return fs.statSync(p).size;
      }
    } catch {
      // ignore
    }
    return 0;
  }

  restoreBackup(key: string): boolean {
    const target = this.getPath(key);
    const bak = path.join(this.baseDir, `${key}.json.bak`);
    try {
      if (fs.existsSync(bak)) {
        fs.copyFileSync(bak, target);
        return true;
      }
    } catch {
      // ignore
    }
    return false;
  }
}
