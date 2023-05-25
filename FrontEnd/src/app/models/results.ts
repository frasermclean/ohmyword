import { WordHintResponse } from './responses/word-hint.response';

/**
 * Result of a player registering for a game.
 */
export interface PlayerRegisteredResult {
  isSuccessful: boolean;
  playerId: string;
  playerCount: number;
  score: number;
  registrationCount: number;
  stateSnapshot: StateSnapshot;
}

/**
 * Result of a player submitting a guess.
 */
export interface SubmitGuessResult {
  isCorrect: boolean;
  pointsAwarded: number;
}

/**
 * Snapshot of the current game state.
 */
interface StateSnapshot {
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  intervalStart: string;
  intervalEnd: string;
  wordHint: WordHintResponse | null;
}
