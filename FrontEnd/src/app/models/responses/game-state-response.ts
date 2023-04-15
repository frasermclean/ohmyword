import { WordHintResponse } from './word-hint.response';
import { RoundSummaryResponse } from './round-summary.response';

export interface GameStateResponse {
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  intervalStart: string;
  intervalEnd: string;
  wordHint: WordHintResponse | null;
  roundSummary: RoundSummaryResponse | null;
}
