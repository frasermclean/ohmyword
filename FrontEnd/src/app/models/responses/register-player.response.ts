import { GameStateResponse } from './game-state.response';

export interface RegisterPlayerResponse {
  playerCount: number;
  score: number;
  gameState: GameStateResponse;
}
