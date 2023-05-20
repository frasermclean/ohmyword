import { LetterHintResponse } from '@models/responses/letter-hint.response';
import { RegisterPlayerResponse } from '@models/responses/register-player.response';
import { RoundEndedModel } from "@models/round-ended.model";
import { RoundStartedModel } from "@models/round-started.model";

/**
 * Game actions and events
 */
export namespace Game {
  export class RegisterPlayer {
    static readonly type = '[Hub State] Game.RegisterPlayer';
  }

  export class AddPoints {
    static readonly type = '[Guess State] Game.AddPoints';
    constructor(public points: number) {}
  }

  export class PlayerRegistered {
    static readonly type = '[Game Service] Game.PlayerRegistered';

    playerCount = this.response.playerCount;
    score = this.response.score;
    gameState = this.response.gameState;

    constructor(private response: RegisterPlayerResponse) {}
  }

  export class RoundStarted {
    static readonly type = '[Game Service] Game.RoundStarted';
    constructor(public data: RoundStartedModel) {}
  }

  export class RoundEnded {
    static readonly type = '[Game Service] Game.RoundEnded';
    constructor(public data: RoundEndedModel) {}
  }

  export class LetterHintReceived {
    static readonly type = '[Game Service] Game.LetterHintReceived';

    position = this.response.position;
    value = this.response.value;

    constructor(private response: LetterHintResponse) {}
  }

  export class PlayerCountUpdated {
    static readonly type = '[Game Service] Game.PlayerCountUpdated';
    constructor(public count: number) {}
  }
}
