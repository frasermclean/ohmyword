import { WordHintResponse } from './word-hint.response';

export interface RoundStartResponse {
  roundId: string;
  roundNumber: number;
  roundEnds: string;
  wordHint: WordHintResponse;
}
