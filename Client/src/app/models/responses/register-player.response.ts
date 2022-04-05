import { GameStatusResponse } from './game-status.response';

export interface RegisterPlayerResponse {
  success: boolean;
  playerId: string;
  status: GameStatusResponse;
}
