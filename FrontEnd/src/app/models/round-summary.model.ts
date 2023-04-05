import { RoundEndReason } from './enums/round-end-reason';

export class RoundSummary {
  readonly word: string;
  readonly endReason: RoundEndReason;
  readonly nextRoundStart: Date;

  constructor(word: string, endReason: RoundEndReason, nextRoundStart: Date) {
    this.word = word;
    this.endReason = endReason;
    this.nextRoundStart = nextRoundStart;
  }
}
