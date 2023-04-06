import { PartOfSpeech } from './enums/part-of-speech.enum';
import { RoundEndReason } from './enums/round-end-reason';
import { RoundSummaryResponse } from './responses/round-summary.response';

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
