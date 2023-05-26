import { LetterHintResponse } from '@models/responses/letter-hint.response';
import { RegisterPlayerResult } from '@models/results';
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

  export class RegisterPlayerSucceeded {
    static readonly type = '[Game Service] Game.RegisterPlayerSuccess';
    constructor(public data: RegisterPlayerResult) {}
  }

  export class RegisterPlayerFailed {
    static readonly type = '[Game Service] Game.RegisterPlayerFailed';
    constructor(public error?: any) {}
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
