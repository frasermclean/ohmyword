import { RoundEndReason } from './enums/round-end-reason';

export class RoundEndSummary {
  readonly word: string;
  readonly endReason: RoundEndReason;
  readonly nextRoundStart: Date;

  constructor(word: string, endReason: RoundEndReason, nextRoundStart: Date) {
    this.word = word;
    this.endReason = endReason;
    this.nextRoundStart = nextRoundStart;
  }

  static default = new RoundEndSummary('', RoundEndReason.Timeout, new Date());
}
