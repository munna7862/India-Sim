import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { DiagnosticsService } from '../../src/domain/services/DiagnosticsService';

describe('Diagnostics & System Health', () => {
  let tempDir: string;

  beforeEach(() => {
    tempDir = path.join(os.tmpdir(), `rashtra_diag_test_${Date.now()}_${Math.random().toString(36).substring(2, 8)}`);
    fs.mkdirSync(tempDir, { recursive: true });
  });

  afterEach(() => {
    try {
      if (fs.existsSync(tempDir)) {
        fs.rmSync(tempDir, { recursive: true, force: true });
      }
    } catch {
      // ignore
    }
  });

  it('DiagnosticsService_GeneratesCompleteSystemReport', () => {
    const service = new DiagnosticsService(tempDir, tempDir);
    const contentRoot = path.join(process.cwd(), 'content');

    const report = service.generateReport(contentRoot);

    expect(report.appVersion).toBe('1.0.0');
    expect(report.appName).toBe('Rashtra: Sovereign Republic');
    expect(report.osDescription.length).toBeGreaterThan(0);
    expect(report.saveDirectoryWritable).toBe(true);
    expect(report.contentStatus.exists).toBe(true);

    const json = service.serializeReportJson(report);
    expect(json).toContain('"appVersion": "1.0.0"');
    expect(json).toContain('"saveDirectoryWritable": true');
  });

  it('DiagnosticsService_LogCrash_CreatesSanitizedTimestampedFile', () => {
    const service = new DiagnosticsService(tempDir, tempDir);
    const err = new Error('Simulation invariant triggered.');

    const logPath = service.logCrash(err, 'Test execution of military battle resolution');

    expect(fs.existsSync(logPath)).toBe(true);
    const content = fs.readFileSync(logPath, 'utf-8');

    expect(content).toContain('=== Rashtra: Sovereign Republic Crash Diagnostic Dump ===');
    expect(content).toContain('Version: 1.0.0');
    expect(content).toContain('Simulation invariant triggered.');
    expect(content).toContain('Context: Test execution of military battle resolution');
  });

  it('DiagnosticsService_RestoreSaveFromBackup_RecoversCorruptedFile', () => {
    const service = new DiagnosticsService(tempDir, tempDir);
    const slotId = 'slot_1';

    const primarySave = path.join(tempDir, `${slotId}.json`);
    const backupSave = path.join(tempDir, `${slotId}.json.bak`);

    fs.writeFileSync(backupSave, JSON.stringify({ validBackupData: true, treasury: 5_000_000_000 }), 'utf-8');
    fs.writeFileSync(primarySave, '{ CORRUPTED_GARBAGE_PAYLOAD', 'utf-8');

    const restored = service.restoreSaveFromBackup(slotId);
    expect(restored).toBe(true);

    const recovered = fs.readFileSync(primarySave, 'utf-8');
    expect(recovered).toContain('"validBackupData":true');
    expect(recovered).toContain('5000000000');
  });
});
