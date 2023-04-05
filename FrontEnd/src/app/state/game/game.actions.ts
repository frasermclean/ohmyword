import { GameStateResponse } from '@models/responses/game-state-response';
import { LetterHintResponse } from '@models/responses/letter-hint.response';
import { RegisterPlayerResponse } from '@models/responses/register-player.response';
import { WordHint } from '@models/word-hint.model';

/**
 * Hub actions and events
 */
export namespace Hub {
  export class Connect {
    static readonly type = '[Game Container] Hub.Connect';
  }

  export class Disconnect {
    static readonly type = '[Game Container] Hub.Disconnect';
  }
  export class Connected {
    static readonly type = '[Game Service] Hub.Connected';
  }

  export class Disconnected {
    static readonly type = '[Game Service] Hub.Disconnected';
    constructor(public error?: Error) {}
  }

  export class ConnectFailed {
    static readonly type = '[Game Service] Hub.ConnectFailed';
    constructor(public error?: any) {}
  }
}

/**
 * Game actions and events
 */
export namespace Game {
  export class PlayerRegistered {
    static readonly type = '[Game Service] Game.PlayerRegistered';

    playerCount = this.response.playerCount;
    score = this.response.score;
    gameState = this.response.gameState;

    constructor(private response: RegisterPlayerResponse) {}
  }

  export class GameStateUpdated {
    static readonly type = '[Game Service] Game.GameStateUpdated';

    roundActive = this.response.roundActive;
    roundNumber = this.response.roundNumber;
    roundId = this.response.roundId;
    intervalStart = new Date(this.response.intervalStart);
    intervalEnd = new Date(this.response.intervalEnd);
    wordHint = new WordHint(this.response.wordHint);

    constructor(private response: GameStateResponse) {}
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

  export class AddPoints {
    static readonly type = '[Guess State] Game.AddPoints';
    constructor(public points: number) {}
  }
}


