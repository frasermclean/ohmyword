import { WordHintResponse } from './word-hint.response';

export interface RegisterPlayerResponse {
  playerId: string;
  roundNumber: number;
  roundActive: boolean;
  wordHint: WordHintResponse | null;
  playerCount: number;
  expiry: string;
}
