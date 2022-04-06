import { GameStatusResponse } from './game-status.response';
import { WordHintResponse } from './word-hint.response';

export interface RegisterPlayerResponse {
  playerId: string;
  gameStatus: GameStatusResponse;
  wordHint: WordHintResponse;
  playerCount: number;
}
