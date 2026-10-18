import { describe, it, expect, beforeEach, afterEach } from 'vitest';
import * as fs from 'fs';
import * as path from 'path';
import * as os from 'os';
import { GameSession } from '../../src/domain/models/GameSession';
import { SaveManager, FileSaveStorage, MemorySaveStorage } from '../../src/domain/services/SaveManager';
import { SaveErrorCode } from '../../src/domain/services/SaveModels';
import { DateOnly } from '../../src/domain/models/DateOnly';
import { ClockSpeed, EventCategory, EventSeverity, GameTaskStatus } from '../../src/domain/models/types';
import { TimedOperation } from '../../src/domain/models/TimedOperation';
import { GameTask, TaskCategory } from '../../src/domain/tasks/GameTask';

describe('SaveManager Persistence & Integrity', () => {
  let tempDir: string;
  let saveManager: SaveManager;

  beforeEach(() => {
    tempDir = path.join(os.tmpdir(), `rashtra_test_${Date.now()}_${Math.random().toString(36).substring(2, 8)}`);
    fs.mkdirSync(tempDir, { recursive: true });
    saveManager = new SaveManager(new FileSaveStorage(tempDir));
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

  it('SaveAndLoad_SlotRoundTrip_PreservesAllAuthoritativeState', () => {
    const session = GameSession.createNewGame('IND');
    const india = session.getPlayerCountry()!;

    // Modify diverse state properties
    india.treasury = 88_500_000_000;
    india.debt = 12_000_000_000;
    india.creditRating = 'AA+';
    india.corruptionIndex = 18.5;
    india.inflationRate = 0.042;
    india.incomeTaxRate = 0.22;
    india.corporateTaxRate = 0.28;
    india.vatRate = 0.14;
    india.facilities['Hospitals'] = 45;
    india.facilities['PowerPlants'] = 38;

    session.clock.currentDate = new DateOnly(2027, 8, 20);
    session.clock.speed = ClockSpeed.Fast;
    session.clock.accumulatorSeconds = 0.25;

    session.addEvent({
      id: 'EVT-TEST-01',
      date: new DateOnly(2027, 8, 15),
      title: 'Diplomatic Milestone',
      message: 'Trade delegation arrived.',
      category: EventCategory.Diplomacy,
      severity: EventSeverity.Info,
      actorCountryId: 'IND',
    });

    const task: GameTask = {
      id: 'TSK_INFRA_SAVE_TEST',
      title: 'Save Test Task',
      category: TaskCategory.Economic,
      status: GameTaskStatus.Completed,
      isRewardClaimed: true,
      subTasks: [
        { id: 'ST_POWER', description: 'Build Power Plants', targetCount: 30, currentCount: 38 },
      ],
      reward: { treasuryBonus: 0, approvalBonus: 0, stabilityBonus: 0 },
    };
    session.tasks[task.id] = task;
    session.claimedRewardIds.push(task.id);

    session.timedOperations.push(
      new TimedOperation(
        'IND',
        'Expanding Defense Grid',
        'Construction',
        new DateOnly(2027, 8, 1),
        new DateOnly(2027, 9, 1),
        31
      )
    );

    session.cooldowns.setCooldown('IND', 'ArmyDrills', new DateOnly(2027, 12, 1));

    // Act: Save
    const saveResult = saveManager.save('slot_1', session);

    // Assert Save
    expect(saveResult.isSuccess).toBe(true);
    expect(saveResult.metadata).toBeDefined();
    expect(saveResult.metadata?.slotId).toBe('slot_1');
    expect(saveResult.metadata?.countryName).toBe('Republic of India');
    expect(saveResult.metadata?.inGameDate.equals(new DateOnly(2027, 8, 20))).toBe(true);

    // Act: Load
    const loadResult = saveManager.load('slot_1');

    // Assert Load
    expect(loadResult.isSuccess).toBe(true);
    const loaded = loadResult.session;
    expect(loaded).toBeDefined();
    expect(loaded?.sessionId).toBe(session.sessionId);
    expect(loaded?.playerCountryId).toBe(session.playerCountryId);
    expect(loaded?.clock.currentDate.equals(session.clock.currentDate)).toBe(true);
    expect(loaded?.clock.speed).toBe(session.clock.speed);

    const loadedIndia = loaded?.getPlayerCountry();
    expect(loadedIndia).toBeDefined();
    expect(loadedIndia?.treasury).toBe(india.treasury);
    expect(loadedIndia?.debt).toBe(india.debt);
    expect(loadedIndia?.creditRating).toBe(india.creditRating);
    expect(loadedIndia?.corruptionIndex).toBe(india.corruptionIndex);
    expect(loadedIndia?.inflationRate).toBe(india.inflationRate);
    expect(loadedIndia?.incomeTaxRate).toBe(india.incomeTaxRate);
    expect(loadedIndia?.facilities['Hospitals']).toBe(45);
    expect(loadedIndia?.facilities['PowerPlants']).toBe(38);

    // Verify Events
    expect(loaded?.events.length).toBe(1);
    expect(loaded?.events[0].id).toBe('EVT-TEST-01');
    expect(loaded?.events[0].title).toBe('Diplomatic Milestone');

    // Verify Tasks
    expect(loaded?.tasks['TSK_INFRA_SAVE_TEST']).toBeDefined();
    expect(loaded?.tasks['TSK_INFRA_SAVE_TEST'].isRewardClaimed).toBe(true);
    expect(loaded?.claimedRewardIds).toContain('TSK_INFRA_SAVE_TEST');

    // Verify Cooldowns
    expect(loaded?.cooldowns.isOnCooldown('IND', 'ArmyDrills', new DateOnly(2027, 8, 20))).toBe(true);
  });

  it('Save_AtomicWrite_CreatesBackupWhenOverwriting', () => {
    const session = GameSession.createNewGame('IND');
    session.clock.currentDate = new DateOnly(2026, 1, 1);
    saveManager.save('slot_2', session);

    const savePath = path.join(tempDir, 'slot_2.json');
    const backupPath = path.join(tempDir, 'slot_2.json.bak');

    expect(fs.existsSync(savePath)).toBe(true);
    expect(fs.existsSync(backupPath)).toBe(false);

    // Act: Save again with updated date
    session.clock.currentDate = new DateOnly(2026, 6, 15);
    saveManager.save('slot_2', session);

    // Assert: Backup file created with initial state
    expect(fs.existsSync(savePath)).toBe(true);
    expect(fs.existsSync(backupPath)).toBe(true);

    const backupJson = fs.readFileSync(backupPath, 'utf-8');
    expect(backupJson).toContain('2026');

    const currentJson = fs.readFileSync(savePath, 'utf-8');
    expect(currentJson).toContain('2026');
  });

  it('Load_NonExistentSlot_ReturnsFileNotFound', () => {
    const result = saveManager.load('non_existent_slot');
    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe(SaveErrorCode.FileNotFound);
  });

  it('Load_CorruptedFile_ReturnsCorruptedSaveDataWithoutCrash', () => {
    const filePath = path.join(tempDir, 'corrupt_slot.json');
    fs.writeFileSync(filePath, '{ corrupted json: invalid syntax ', 'utf-8');

    const result = saveManager.load('corrupt_slot');
    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe(SaveErrorCode.CorruptedSaveData);
  });

  it('Load_NewerSchemaVersion_ReturnsSaveTooNew', () => {
    const session = GameSession.createNewGame('IND');
    session.schemaVersion = 999;
    saveManager.save('future_slot', session);

    const result = saveManager.load('future_slot');
    expect(result.isSuccess).toBe(false);
    expect(result.errorCode).toBe(SaveErrorCode.SaveTooNew);
  });

  it('QuickSave_And_QuickLoad_WorkSeamlessly', () => {
    const session = GameSession.createNewGame('IND');
    session.clock.currentDate = new DateOnly(2026, 5, 10);

    const saveResult = saveManager.quickSave(session);
    const loadResult = saveManager.quickLoad();

    expect(saveResult.isSuccess).toBe(true);
    expect(loadResult.isSuccess).toBe(true);
    expect(loadResult.session?.clock.currentDate.equals(new DateOnly(2026, 5, 10))).toBe(true);
  });

  it('ListSlots_ReturnsAllSlotsWithCorrectMetadata', () => {
    const session1 = GameSession.createNewGame('IND');
    session1.clock.currentDate = new DateOnly(2026, 2, 1);
    saveManager.save('slot_1', session1);

    const session2 = GameSession.createNewGame('IND');
    session2.clock.currentDate = new DateOnly(2026, 3, 1);
    saveManager.save('slot_2', session2);

    const slots = saveManager.listSlots();
    expect(slots.length).toBe(2);
    expect(slots.some((s) => s.slotId === 'slot_1')).toBe(true);
    expect(slots.some((s) => s.slotId === 'slot_2')).toBe(true);
  });

  it('DeleteSlot_RemovesFilesSuccessfully', () => {
    const session = GameSession.createNewGame('IND');
    saveManager.save('delete_test', session);
    expect(fs.existsSync(path.join(tempDir, 'delete_test.json'))).toBe(true);

    const deleted = saveManager.deleteSlot('delete_test');
    expect(deleted).toBe(true);
    expect(fs.existsSync(path.join(tempDir, 'delete_test.json'))).toBe(false);
  });
});
