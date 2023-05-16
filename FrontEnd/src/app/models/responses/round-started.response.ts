import { WordHintResponse } from "@models/responses/word-hint.response";

export interface RoundStartedResponse {
  roundNumber: number;
  roundId: string;
  wordHint: WordHintResponse;
  startTime: string;
  endTime: string;
}
