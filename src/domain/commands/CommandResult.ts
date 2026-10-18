import { GameSession } from '../models/GameSession';

export class CommandResult {
  isSuccess: boolean;
  errorCode: string | null;
  errorMessage: string | null;

  constructor(isSuccess: boolean, errorCode: string | null = null, errorMessage: string | null = null) {
    this.isSuccess = isSuccess;
    this.errorCode = errorCode;
    this.errorMessage = errorMessage;
  }

  static success(): CommandResult {
    return new CommandResult(true);
  }

  static failure(errorCode: string, errorMessage: string): CommandResult {
    return new CommandResult(false, errorCode, errorMessage);
  }
}

export interface ICommand {
  execute(session: GameSession): CommandResult;
}
