import { PartOfSpeech, RoundEndReason } from "@models/enums";

export interface RoundSummaryResponse {
  word: string;
  partOfSpeech: PartOfSpeech;
  endReason: RoundEndReason;
}
