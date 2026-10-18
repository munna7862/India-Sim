import * as fs from 'fs';
import * as path from 'path';
import { GameSession } from '../models/GameSession';
import { DateOnly } from '../models/DateOnly';
import { SimulationClock } from '../models/SimulationClock';
import { CountryState } from '../models/CountryState';
import { DiplomaticStance } from '../models/Diplomacy';
import { CooldownTracker } from '../models/Cooldowns';
import { MinistryState, FundingLevel } from '../models/MinistryState';
import { GameTask, SubTaskState } from '../tasks/GameTask';
import {
  SaveErrorCode,
  SaveSlotMetadata,
  SaveSlotResult,
  LoadSlotResult,
} from './SaveModels';

export interface ISaveStorage {
  getItem(key: string): string | null;
  setItem(key: string, value: string): void;
  removeItem(key: string): void;
  hasItem(key: string): boolean;
  listKeys(): string[];
  getSize?(key: string): number;
}

export class MemorySaveStorage implements ISaveStorage {
  private storage = new Map<string, string>();

  getItem(key: string): string | null {
    return this.storage.get(key) ?? null;
  }
  setItem(key: string, value: string): void {
    this.storage.set(key, value);
  }
  removeItem(key: string): void {
    this.storage.delete(key);
  }
  hasItem(key: string): boolean {
    return this.storage.has(key);
  }
  listKeys(): string[] {
    return Array.from(this.storage.keys());
  }
  getSize(key: string): number {
    const val = this.storage.get(key);
    return val ? new TextEncoder().encode(val).length : 0;
  }
}

export class LocalStorageSaveStorage implements ISaveStorage {
  private prefix = 'india_sim_save_';

  getItem(key: string): string | null {
    try {
      return localStorage.getItem(this.prefix + key);
    } catch {
      return null;
    }
  }
  setItem(key: string, value: string): void {
    try {
      localStorage.setItem(this.prefix + key, value);
    } catch (e) {
      console.warn('LocalStorage save failed:', e);
    }
  }
  removeItem(key: string): void {
    try {
      localStorage.removeItem(this.prefix + key);
    } catch {
      // ignore
    }
  }
  hasItem(key: string): boolean {
    return this.getItem(key) !== null;
  }
  listKeys(): string[] {
    const keys: string[] = [];
    try {
      for (let i = 0; i < localStorage.length; i++) {
        const k = localStorage.key(i);
        if (k && k.startsWith(this.prefix) && !k.endsWith('.bak') && !k.endsWith('.tmp')) {
          keys.push(k.substring(this.prefix.length));
        }
      }
    } catch {
      // ignore
    }
    return keys;
  }
  getSize(key: string): number {
    const val = this.getItem(key);
    return val ? new TextEncoder().encode(val).length : 0;
  }
}

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

export class SaveManager {
  public static readonly CURRENT_SCHEMA_VERSION = 1;
  public static readonly QUICK_SAVE_SLOT = 'quicksave';
  public static readonly AUTO_SAVE_SLOT = 'autosave';

  private storage: ISaveStorage;
  private saveDir?: string;

  constructor(customDirectoryOrStorage?: string | ISaveStorage) {
    if (typeof customDirectoryOrStorage === 'object' && customDirectoryOrStorage !== null) {
      this.storage = customDirectoryOrStorage;
    } else if (typeof customDirectoryOrStorage === 'string') {
      this.saveDir = customDirectoryOrStorage;
      this.storage = new FileSaveStorage(customDirectoryOrStorage);
    } else {
      // Auto-detect environment
      if (typeof window !== 'undefined' && window.localStorage) {
        this.storage = new LocalStorageSaveStorage();
      } else {
        // Node environment fallback
        try {
          const appData =
            process.env.APPDATA ||
            (process.platform === 'darwin'
              ? `${process.env.HOME}/Library/Application Support`
              : `${process.env.HOME}/.local/share`);
          const defaultDir = path.join(appData || './', 'RashtraSovereignRepublic', 'saves');
          this.saveDir = defaultDir;
          this.storage = new FileSaveStorage(defaultDir);
        } catch {
          this.storage = new MemorySaveStorage();
        }
      }
    }
  }

  public getSaveDirectory(): string | undefined {
    return this.saveDir;
  }

  public sanitizeSlotId(slotId: string): string {
    return slotId.replace(/[^a-zA-Z0-9_\-]/g, '_').toLowerCase();
  }

  public formatSlotDisplayName(slotId: string): string {
    const s = slotId.toLowerCase();
    if (s === SaveManager.QUICK_SAVE_SLOT) return 'Quick Save (F5)';
    if (s === SaveManager.AUTO_SAVE_SLOT) return 'Auto Save';
    if (s === 'slot_1') return 'Save Slot 1';
    if (s === 'slot_2') return 'Save Slot 2';
    if (s === 'slot_3') return 'Save Slot 3';
    if (s === 'slot_4') return 'Save Slot 4';
    if (s === 'slot_5') return 'Save Slot 5';
    return `Slot ${slotId}`;
  }

  public serializeSession(session: GameSession): string {
    return JSON.stringify(session, null, 2);
  }

  public deserializeSession(json: string): GameSession {
    const parsed = JSON.parse(json);
    return this.hydrateSession(parsed);
  }

  private parseDateOnly(val: any): DateOnly {
    if (!val) return new DateOnly(2026, 1, 1);
    if (val instanceof DateOnly) return val;
    if (typeof val === 'string') return DateOnly.fromString(val);
    if (typeof val === 'object' && val.year !== undefined && val.month !== undefined && val.day !== undefined) {
      return new DateOnly(val.year, val.month, val.day);
    }
    return new DateOnly(2026, 1, 1);
  }

  private hydrateSession(raw: any): GameSession {
    const session = new GameSession();
    if (raw.sessionId) session.sessionId = raw.sessionId;
    if (raw.playerCountryId) session.playerCountryId = raw.playerCountryId;
    session.schemaVersion = raw.schemaVersion ?? 1;

    // Clock hydration
    if (raw.clock) {
      session.clock.currentDate = this.parseDateOnly(raw.clock.currentDate);
      session.clock.speed = raw.clock.speed ?? session.clock.speed;
      session.clock.isPaused = raw.clock.isPaused ?? false;
      session.clock.totalDaysSimulated = raw.clock.totalDaysSimulated ?? 0;
      session.clock.accumulatorSeconds = raw.clock.accumulatorSeconds ?? 0;
    }

    // Countries hydration
    if (raw.countries && typeof raw.countries === 'object') {
      session.countries = {};
      for (const [id, c] of Object.entries<any>(raw.countries)) {
        const country = new CountryState(c.id, c.name, c.isPlayerControlled);
        Object.assign(country, c);

        // Hydrate timed operations in country
        if (Array.isArray(c.timedOperations)) {
          country.timedOperations = c.timedOperations.map((op: any) => ({
            ...op,
            startDate: this.parseDateOnly(op.startDate),
            targetCompletionDate: this.parseDateOnly(op.targetCompletionDate || op.completionDate),
          }));
        }

        // Hydrate diplomatic relations
        if (c.diplomaticRelations && typeof c.diplomaticRelations === 'object') {
          country.diplomaticRelations = {};
          for (const [relId, stanceRaw] of Object.entries<any>(c.diplomaticRelations)) {
            const contracts = Array.isArray(stanceRaw.activeTradeContracts)
              ? stanceRaw.activeTradeContracts.map((tc: any) => ({
                  ...tc,
                  startDate: this.parseDateOnly(tc.startDate),
                  expiryDate: tc.expiryDate ? this.parseDateOnly(tc.expiryDate) : null,
                }))
              : [];

            const stance = new DiplomaticStance(
              stanceRaw.relations ?? 0,
              stanceRaw.isAtWar ?? false,
              stanceRaw.nonAggressionPactUntil ? this.parseDateOnly(stanceRaw.nonAggressionPactUntil) : null,
              stanceRaw.hasAlliance ?? false,
              contracts
            );
            country.diplomaticRelations[relId] = stance;
          }
        }

        // Hydrate ministries
        if (c.ministries && typeof c.ministries === 'object') {
          country.ministries = {};
          for (const [mType, mRaw] of Object.entries<any>(c.ministries)) {
            country.ministries[mType] = new MinistryState(
              mRaw.type ?? mType,
              mRaw.activeMinister ? { ...mRaw.activeMinister } : null,
              mRaw.funding ?? mRaw.fundingLevel ?? FundingLevel.Medium
            );
          }
        }

        session.countries[id] = country;
      }
    }

    // Events
    if (Array.isArray(raw.events)) {
      session.events = raw.events.map((ev: any) => ({
        ...ev,
        date: this.parseDateOnly(ev.date),
      }));
    }

    // Timed Operations
    if (Array.isArray(raw.timedOperations)) {
      session.timedOperations = raw.timedOperations.map((op: any) => ({
        ...op,
        startDate: this.parseDateOnly(op.startDate),
        completionDate: this.parseDateOnly(op.completionDate || op.targetCompletionDate),
        targetCompletionDate: this.parseDateOnly(op.targetCompletionDate || op.completionDate),
      }));
    }

    // Cooldowns
    session.cooldowns = new CooldownTracker();
    if (raw.cooldowns && raw.cooldowns.cooldowns && typeof raw.cooldowns.cooldowns === 'object') {
      for (const [k, v] of Object.entries<any>(raw.cooldowns.cooldowns)) {
        session.cooldowns.cooldowns[k] = this.parseDateOnly(v);
      }
    }

    // Active domestic events
    if (Array.isArray(raw.activeDomesticEvents)) {
      session.activeDomesticEvents = raw.activeDomesticEvents.map((dev: any) => ({
        ...dev,
        triggerDate: this.parseDateOnly(dev.triggerDate),
      }));
    }

    // Pending event
    if (raw.pendingEvent) {
      session.pendingEvent = {
        ...raw.pendingEvent,
        date: this.parseDateOnly(raw.pendingEvent.date),
      };
    }

    // Event cooldowns
    if (raw.eventCooldowns && typeof raw.eventCooldowns === 'object') {
      session.eventCooldowns = { ...raw.eventCooldowns };
    }

    // Tasks & Achievements
    if (raw.tasks && typeof raw.tasks === 'object') {
      session.tasks = {};
      for (const [id, tRaw] of Object.entries<any>(raw.tasks)) {
        const task = new GameTask();
        Object.assign(task, tRaw);
        if (Array.isArray(tRaw.subTasks)) {
          task.subTasks = tRaw.subTasks.map(
            (st: any) => new SubTaskState(st.id, st.description, st.targetCount, st.currentCount)
          );
        }
        session.tasks[id] = task;
      }
    }
    if (Array.isArray(raw.claimedRewardIds)) {
      session.claimedRewardIds = [...raw.claimedRewardIds];
    }
    if (raw.achievements && typeof raw.achievements === 'object') {
      session.achievements = raw.achievements;
    }
    if (Array.isArray(raw.claimedAchievementRewardIds)) {
      session.claimedAchievementRewardIds = [...raw.claimedAchievementRewardIds];
    }

    return session;
  }

  public save(slotId: string, session: GameSession): SaveSlotResult {
    if (!slotId || slotId.trim().length === 0) {
      return SaveSlotResult.failure(slotId, SaveErrorCode.IoError, 'Slot ID cannot be empty');
    }

    const cleanSlot = this.sanitizeSlotId(slotId);
    try {
      const json = this.serializeSession(session);
      this.storage.setItem(cleanSlot, json);

      const player = session.getPlayerCountry();
      const bytes = this.storage.getSize ? this.storage.getSize(cleanSlot) : json.length;

      const metadata: SaveSlotMetadata = {
        slotId: cleanSlot,
        displayName: this.formatSlotDisplayName(cleanSlot),
        countryId: session.playerCountryId,
        countryName: player?.name ?? session.playerCountryId,
        inGameDate: session.clock.currentDate,
        realTimeSavedUtc: new Date().toISOString(),
        schemaVersion: session.schemaVersion,
        isCorrupted: false,
        errorMessage: null,
        fileSizeBytes: bytes,
      };

      return SaveSlotResult.success(cleanSlot, metadata);
    } catch (err: any) {
      return SaveSlotResult.failure(
        cleanSlot,
        SaveErrorCode.IoError,
        `Failed to save slot '${cleanSlot}': ${err?.message || err}`
      );
    }
  }

  public load(slotId: string): LoadSlotResult {
    if (!slotId || slotId.trim().length === 0) {
      return LoadSlotResult.failure(slotId, SaveErrorCode.FileNotFound, 'Slot ID cannot be empty');
    }

    const cleanSlot = this.sanitizeSlotId(slotId);
    if (!this.storage.hasItem(cleanSlot)) {
      return LoadSlotResult.failure(
        cleanSlot,
        SaveErrorCode.FileNotFound,
        `Save slot '${cleanSlot}' does not exist.`
      );
    }

    try {
      const json = this.storage.getItem(cleanSlot);
      if (!json) {
        return LoadSlotResult.failure(
          cleanSlot,
          SaveErrorCode.FileNotFound,
          `Save slot '${cleanSlot}' is empty or missing.`
        );
      }

      let parsed: any;
      try {
        parsed = JSON.parse(json);
      } catch (parseErr: any) {
        return LoadSlotResult.failure(
          cleanSlot,
          SaveErrorCode.CorruptedSaveData,
          `Corrupted save file in slot '${cleanSlot}': ${parseErr.message}`
        );
      }

      if (parsed.schemaVersion && parsed.schemaVersion > SaveManager.CURRENT_SCHEMA_VERSION) {
        return LoadSlotResult.failure(
          cleanSlot,
          SaveErrorCode.SaveTooNew,
          `Save schema version ${parsed.schemaVersion} is newer than current supported version ${SaveManager.CURRENT_SCHEMA_VERSION}.`
        );
      }

      const session = this.hydrateSession(parsed);
      return LoadSlotResult.success(cleanSlot, session);
    } catch (err: any) {
      return LoadSlotResult.failure(
        cleanSlot,
        SaveErrorCode.IoError,
        `Failed to read save slot '${cleanSlot}': ${err.message}`
      );
    }
  }

  public quickSave(session: GameSession): SaveSlotResult {
    return this.save(SaveManager.QUICK_SAVE_SLOT, session);
  }

  public quickLoad(): LoadSlotResult {
    return this.load(SaveManager.QUICK_SAVE_SLOT);
  }

  public autoSave(session: GameSession): SaveSlotResult {
    return this.save(SaveManager.AUTO_SAVE_SLOT, session);
  }

  public deleteSlot(slotId: string): boolean {
    const clean = this.sanitizeSlotId(slotId);
    if (this.storage.hasItem(clean)) {
      this.storage.removeItem(clean);
      return true;
    }
    return false;
  }

  public getSlotMetadata(slotId: string): SaveSlotMetadata | null {
    const clean = this.sanitizeSlotId(slotId);
    if (!this.storage.hasItem(clean)) return null;

    try {
      const json = this.storage.getItem(clean);
      if (!json) return null;
      const parsed = JSON.parse(json);
      const session = this.hydrateSession(parsed);
      const player = session.getPlayerCountry();
      const bytes = this.storage.getSize ? this.storage.getSize(clean) : json.length;

      return {
        slotId: clean,
        displayName: this.formatSlotDisplayName(clean),
        countryId: session.playerCountryId,
        countryName: player?.name ?? session.playerCountryId,
        inGameDate: session.clock.currentDate,
        realTimeSavedUtc: new Date().toISOString(),
        schemaVersion: session.schemaVersion,
        isCorrupted: false,
        errorMessage: null,
        fileSizeBytes: bytes,
      };
    } catch (err: any) {
      return {
        slotId: clean,
        displayName: this.formatSlotDisplayName(clean),
        countryId: '???',
        countryName: 'Corrupted Save',
        inGameDate: new DateOnly(2026, 1, 1),
        realTimeSavedUtc: new Date().toISOString(),
        schemaVersion: 0,
        isCorrupted: true,
        errorMessage: err.message,
        fileSizeBytes: 0,
      };
    }
  }

  public listSlots(): SaveSlotMetadata[] {
    const keys = this.storage.listKeys();
    const result: SaveSlotMetadata[] = [];
    for (const key of keys) {
      const meta = this.getSlotMetadata(key);
      if (meta) {
        result.push(meta);
      }
    }
    return result.sort(
      (a, b) => new Date(b.realTimeSavedUtc).getTime() - new Date(a.realTimeSavedUtc).getTime()
    );
  }

  public restoreSaveFromBackup(slotId: string): boolean {
    const clean = this.sanitizeSlotId(slotId);
    if (this.storage instanceof FileSaveStorage) {
      return this.storage.restoreBackup(clean);
    }
    // Web / Memory fallback: check if `${clean}.bak` exists in storage
    const bakKey = `${clean}.bak`;
    if (this.storage.hasItem(bakKey)) {
      const val = this.storage.getItem(bakKey);
      if (val) {
        this.storage.setItem(clean, val);
        return true;
      }
    }
    return false;
  }
}
