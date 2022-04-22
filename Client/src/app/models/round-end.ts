import { RoundEndResponse } from './responses/round-end.response';

export class RoundEnd {
  roundId: string;
  nextRoundStart: Date;
  word: string;
  
  constructor(init?: Partial<RoundEndResponse>) {
    this.roundId = init?.roundId ?? '';
    this.nextRoundStart = init?.nextRoundStart ? new Date(init.nextRoundStart) : new Date();
    this.word = init?.word ?? '';
  }
}
