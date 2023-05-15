import { RoundEndReason } from "@models/enums";
import { ScoreLineResponse } from "@models/responses/score-line.response";
import { PartOfSpeech } from "@models/enums";

export interface RoundEndedEventResponse {
  word: string;
  partOfSpeech: PartOfSpeech;
  endReason: RoundEndReason;
  nextRoundStart: string;
  scores: ScoreLineResponse[];
}
