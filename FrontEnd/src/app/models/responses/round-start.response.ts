import { WordHintResponse } from './word-hint.response';

export interface RoundStartResponse {
  roundId: string;
  roundNumber: number;
  roundStarted: string;
  roundEnds: string;
  wordHint: WordHintResponse;
}
