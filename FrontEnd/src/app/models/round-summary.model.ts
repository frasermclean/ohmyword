import { RoundSummaryResponse } from './responses/round-summary.response';
import { PartOfSpeech, RoundEndReason } from "@models/enums";

export class RoundSummary {
  readonly word: string;
  readonly partOfSpeech: PartOfSpeech;
  readonly endReason: RoundEndReason;

  constructor(init?: Partial<RoundSummaryResponse>) {
    this.word = init?.word ?? '';
    this.partOfSpeech = init?.partOfSpeech ?? PartOfSpeech.Noun;
    this.endReason = init?.endReason ?? RoundEndReason.Timeout;
  }
}
