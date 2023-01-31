import { WordHintResponse } from "./word-hint.response";

export interface GameStateResponse {
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  intervalStart: string;
  intervalEnd: string;
  wordHint: WordHintResponse;
}
