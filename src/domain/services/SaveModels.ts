import { GameSession } from '../models/GameSession';
import { DateOnly } from '../models/DateOnly';

export enum SaveErrorCode {
  None = 'None',
  FileNotFound = 'FileNotFound',
  CorruptedSaveData = 'CorruptedSaveData',
  SaveTooNew = 'SaveTooNew',
  IoError = 'IoError',
  Unknown = 'Unknown',
}

export interface SaveSlotMetadata {
  slotId: string;
  displayName: string;
  countryId: string;
  countryName: string;
  inGameDate: DateOnly;
  realTimeSavedUtc: string; // ISO string
  schemaVersion: number;
  isCorrupted: boolean;
  errorMessage?: string | null;
  fileSizeBytes: number;
}

export interface SaveSlotResult {
  isSuccess: boolean;
  slotId: string;
  errorCode: SaveErrorCode;
  errorMessage?: string | null;
  metadata?: SaveSlotMetadata | null;
}

export interface LoadSlotResult {
  isSuccess: boolean;
  slotId: string;
  session?: GameSession | null;
  errorCode: SaveErrorCode;
  errorMessage?: string | null;
}

export const SaveSlotResult = {
  success: (slotId: string, metadata: SaveSlotMetadata): SaveSlotResult => ({
    isSuccess: true,
    slotId,
    errorCode: SaveErrorCode.None,
    errorMessage: null,
    metadata,
  }),
  failure: (slotId: string, code: SaveErrorCode, message: string): SaveSlotResult => ({
    isSuccess: false,
    slotId,
    errorCode: code,
    errorMessage: message,
    metadata: null,
  }),
};

export const LoadSlotResult = {
  success: (slotId: string, session: GameSession): LoadSlotResult => ({
    isSuccess: true,
    slotId,
    session,
    errorCode: SaveErrorCode.None,
    errorMessage: null,
  }),
  failure: (slotId: string, code: SaveErrorCode, message: string): LoadSlotResult => ({
    isSuccess: false,
    slotId,
    session: null,
    errorCode: code,
    errorMessage: message,
  }),
};
