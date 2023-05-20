import { WordHintResponse } from "@models/responses/word-hint.response";

export interface RoundStartedModel {
  roundNumber: number;
  roundId: string;
  wordHint: WordHintResponse;
  startDate: string;
  endDate: string;
}
