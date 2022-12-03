import { RoundEndResponse } from './round-end.response';
import { RoundStartResponse } from './round-start.response';

export interface RegisterPlayerResponse {
  roundActive: boolean;
  playerCount: number;
  score: number;
  roundStart: RoundStartResponse | null;
  roundEnd: RoundEndResponse | null;
}
