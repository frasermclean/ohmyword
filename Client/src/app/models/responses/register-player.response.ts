import { WordHintResponse } from './word-hint.response';

export interface RegisterPlayerResponse {
  playerId: string;
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  wordHint: WordHintResponse | null;
  playerCount: number;
  expiry: string;
  score: number;
}
