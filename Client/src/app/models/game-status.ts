import { GameStatusResponse } from './responses/game-status.response';

export class GameStatus {
  readonly roundActive: boolean;
  readonly expiry: Date;
  readonly playerCount: number;

  constructor(init?: Partial<GameStatusResponse>) {
    this.roundActive = init?.roundActive ?? false;
    this.expiry = new Date(init?.expiry ?? 0);
    this.playerCount = init?.playerCount ?? 0;
  }

  public static default = new GameStatus();
}
