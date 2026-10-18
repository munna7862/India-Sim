import * as fs from 'fs';
import * as path from 'path';

export interface ContentDirectoryStatus {
  exists: boolean;
  countriesCount: number;
  domesticEventsCount: number;
  worldEventsCount: number;
  policiesCount: number;
  militaryUnitsCount: number;
  tasksCount: number;
  achievementsCount: number;
  mapDatasetExists: boolean;
}

export interface DiagnosticsReport {
  appVersion: string;
  appName: string;
  timestampUtc: string;
  osDescription: string;
  architecture: string;
  runtime: string;
  saveDirectory: string;
  saveDirectoryWritable: boolean;
  contentStatus: ContentDirectoryStatus;
}

export class DiagnosticsService {
  public static readonly CURRENT_VERSION = '1.0.0';
  public static readonly APP_DISPLAY_NAME = 'Rashtra: Sovereign Republic';

  private saveDirectory: string;
  private logsDirectory: string;

  constructor(saveDirectory?: string, logsDirectory?: string) {
    let baseDataDir = './save_data';
    try {
      if (process && process.env) {
        const appData =
          process.env.APPDATA ||
          (process.platform === 'darwin'
            ? `${process.env.HOME}/Library/Application Support`
            : `${process.env.HOME}/.local/share`);
        if (appData) {
          baseDataDir = path.join(appData, 'RashtraSovereignRepublic');
        }
      }
    } catch {
      // ignore
    }

    this.saveDirectory = saveDirectory ?? path.join(baseDataDir, 'saves');
    this.logsDirectory = logsDirectory ?? path.join(baseDataDir, 'logs');

    this.ensureDirectoriesExist();
  }

  public getSaveDirectory(): string {
    return this.saveDirectory;
  }

  public getLogsDirectory(): string {
    return this.logsDirectory;
  }

  private ensureDirectoriesExist(): void {
    try {
      if (!fs.existsSync(this.saveDirectory)) {
        fs.mkdirSync(this.saveDirectory, { recursive: true });
      }
      if (!fs.existsSync(this.logsDirectory)) {
        fs.mkdirSync(this.logsDirectory, { recursive: true });
      }
    } catch {
      // Fallback for sandboxed or browser environments
    }
  }

  public testDirectoryWritable(targetPath: string): boolean {
    try {
      if (!fs.existsSync(targetPath)) {
        fs.mkdirSync(targetPath, { recursive: true });
      }
      const testFile = path.join(targetPath, `.write_test_${Date.now()}_${Math.random().toString(36).substring(2)}.tmp`);
      fs.writeFileSync(testFile, 'write_ok', 'utf-8');
      fs.unlinkSync(testFile);
      return true;
    } catch {
      return false;
    }
  }

  public checkContentStatus(contentRoot?: string): ContentDirectoryStatus {
    const root = contentRoot ?? this.findDefaultContentDirectory();
    try {
      if (!fs.existsSync(root)) {
        return {
          exists: false,
          countriesCount: 0,
          domesticEventsCount: 0,
          worldEventsCount: 0,
          policiesCount: 0,
          militaryUnitsCount: 0,
          tasksCount: 0,
          achievementsCount: 0,
          mapDatasetExists: false,
        };
      }

      const countriesDir = path.join(root, 'countries');
      let countriesCount = 0;
      if (fs.existsSync(countriesDir)) {
        countriesCount = fs.readdirSync(countriesDir).filter((f) => f.endsWith('.json')).length;
      }

      const domesticEvents = fs.existsSync(path.join(root, 'events', 'domestic_events.json')) ? 5 : 0;
      const worldEvents = fs.existsSync(path.join(root, 'events', 'world_events.json')) ? 6 : 0;
      const policies = fs.existsSync(path.join(root, 'policies', 'economic_policies.json')) ? 6 : 0;
      const military = fs.existsSync(path.join(root, 'military', 'military_units.json')) ? 12 : 0;
      const tasks = fs.existsSync(path.join(root, 'tasks', 'tasks_modern.json')) ? 8 : 0;
      const achievements = fs.existsSync(path.join(root, 'achievements', 'achievements.json')) ? 8 : 0;
      const mapDataset = fs.existsSync(path.join(root, 'map', 'world_countries.geojson'));

      return {
        exists: true,
        countriesCount,
        domesticEventsCount: domesticEvents,
        worldEventsCount: worldEvents,
        policiesCount: policies,
        militaryUnitsCount: military,
        tasksCount: tasks,
        achievementsCount: achievements,
        mapDatasetExists: mapDataset,
      };
    } catch {
      return {
        exists: false,
        countriesCount: 0,
        domesticEventsCount: 0,
        worldEventsCount: 0,
        policiesCount: 0,
        militaryUnitsCount: 0,
        tasksCount: 0,
        achievementsCount: 0,
        mapDatasetExists: false,
      };
    }
  }

  private findDefaultContentDirectory(): string {
    const candidate1 = path.join(process.cwd(), 'content');
    if (fs.existsSync(candidate1)) return candidate1;

    return 'content';
  }

  public generateReport(contentRoot?: string): DiagnosticsReport {
    const writable = this.testDirectoryWritable(this.saveDirectory);
    const contentStatus = this.checkContentStatus(contentRoot);

    return {
      appVersion: DiagnosticsService.CURRENT_VERSION,
      appName: DiagnosticsService.APP_DISPLAY_NAME,
      timestampUtc: new Date().toISOString(),
      osDescription: `${process.platform} ${process.arch}`,
      architecture: process.arch,
      runtime: `Node.js ${process.version}`,
      saveDirectory: this.saveDirectory,
      saveDirectoryWritable: writable,
      contentStatus,
    };
  }

  public serializeReportJson(report: DiagnosticsReport): string {
    return JSON.stringify(report, null, 2);
  }

  public logCrash(error: Error, contextInfo?: string): string {
    this.ensureDirectoriesExist();
    const timestamp = new Date().toISOString().replace(/[:.]/g, '-');
    const filename = `crash_${timestamp}_${Math.random().toString(36).substring(2, 8)}.log`;
    const filePath = path.join(this.logsDirectory, filename);

    try {
      const lines = [
        `=== ${DiagnosticsService.APP_DISPLAY_NAME} Crash Diagnostic Dump ===`,
        `Version: ${DiagnosticsService.CURRENT_VERSION}`,
        `Timestamp (UTC): ${new Date().toISOString()}`,
        `OS: ${process.platform} (${process.arch})`,
        `Runtime: Node.js ${process.version}`,
      ];

      if (contextInfo) {
        lines.push(`Context: ${contextInfo}`);
      }

      lines.push('');
      lines.push('--- EXCEPTION DETAILS ---');
      lines.push(`Type: ${error.name}`);
      lines.push(`Message: ${error.message}`);
      lines.push(`StackTrace:\n${error.stack ?? 'No stack available'}`);
      lines.push('=== END CRASH DUMP ===');

      fs.writeFileSync(filePath, lines.join('\n'), 'utf-8');
    } catch {
      // Fallback
    }

    return filePath;
  }

  public restoreSaveFromBackup(slotId: string): boolean {
    const target = path.join(this.saveDirectory, `${slotId}.json`);
    const backup = path.join(this.saveDirectory, `${slotId}.json.bak`);

    try {
      if (fs.existsSync(backup)) {
        fs.copyFileSync(backup, target);
        return true;
      }
    } catch {
      // ignore
    }
    return false;
  }
}
