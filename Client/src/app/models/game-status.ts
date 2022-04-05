import { GameStatusResponse } from './responses/game-status.response';

export class GameStatus {
  readonly roundActive: boolean;
  readonly expiry: Date;
  readonly playerCount: number;

  constructor(response: GameStatusResponse) {
    this.roundActive = response.roundActive;
    this.expiry = new Date(response.expiry);
    this.playerCount = response.playerCount;
  }

  public static default = new GameStatus({
    roundActive: false,
    expiry: '',
    playerCount: 0,
  })
}
