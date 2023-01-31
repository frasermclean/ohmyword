import { RoundEndResponse } from './round-end.response';
import { RoundStartResponse } from './round-start.response';

export interface RegisterVisitorResponse {
  roundActive: boolean;
  visitorCount: number;
  score: number;
  roundStart: RoundStartResponse | null;
  roundEnd: RoundEndResponse | null;
}
