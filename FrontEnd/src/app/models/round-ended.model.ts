import { RoundEndReason } from "@models/enums";
import { ScoreLineModel } from "@models/score-line.model";

export interface RoundEndedModel {
  word: string;
  endReason: RoundEndReason;
  nextRoundStart: string;
  scores: ScoreLineModel[];
}
