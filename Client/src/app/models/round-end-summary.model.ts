import { RoundEndReason } from './responses/round-end.response';

export class RoundEndSummary {
  readonly word: string;
  readonly endReason: RoundEndReason;

  constructor(word: string, endReason: RoundEndReason) {
    this.word = word;
    this.endReason = endReason;
  }
}
