import { GameStateResponse } from './game-state-response';

export interface RegisterVisitorResponse {
  visitorCount: number;
  score: number;
  gameState: GameStateResponse;
}
