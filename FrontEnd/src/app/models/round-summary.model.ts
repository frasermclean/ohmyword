import { PartOfSpeech, RoundEndReason } from "./enums";

export interface RoundSummary {
  word: string;
  partOfSpeech: PartOfSpeech;
  endReason: RoundEndReason;
  roundId: string;
  definitionId: string;
  nextRoundStart: string;
  scores: ScoreLine[];
}

export interface ScoreLine {
  playerName: string;
  connectionId: string;
  countryName: string;
  countryCode: string;
  pointsAwarded: number;
  guessCount: number;
  guessTimeMilliseconds: number;
}
