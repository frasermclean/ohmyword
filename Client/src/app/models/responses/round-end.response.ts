export interface RoundEndResponse {
  roundNumber: number;
  roundId: string;
  word: string;
  endReason: RoundEndReason;
  nextRoundStart: string;
}

export enum RoundEndReason {
  Timeout = 'timeout',
  AllPlayersAwarded = 'allPlayersAwarded',
  NoPlayersLeft = 'noPlayersLeft',
}
