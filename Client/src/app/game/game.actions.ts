import { LetterHintResponse } from '../models/responses/letter-hint.response';
import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { RoundEndResponse } from '../models/responses/round-end.response';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { WordHint } from '../models/word-hint.model';

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

    playerId = this.response.playerId;
    playerCount = this.response.playerCount;
    roundActive = this.response.roundActive;
    roundNumber = this.response.roundNumber;
    roundId = this.response.roundId;
    expiry = new Date(this.response.expiry);
    wordHint = this.response.wordHint ? new WordHint(this.response.wordHint) : WordHint.default;
    score = this.response.score;

    constructor(private response: RegisterPlayerResponse) {}
  }

  export class RoundStarted {
    static readonly type = '[Game Service] Game.RoundStarted';

    roundId = this.response.roundId;
    roundNumber = this.response.roundNumber;
    roundEnds = this.response.roundEnds;
    wordHint = this.response.wordHint;

    constructor(private response: RoundStartResponse) {}
  }

  export class RoundEnded {
    static readonly type = '[Game Service] Game.RoundEnded';

    roundId = this.response.roundId;
    nextRoundStart = new Date(this.response.nextRoundStart);
    word = this.response.word;

    constructor(private response: RoundEndResponse) {}
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

/**
 * Guess actions and events
 */
export namespace Guess {
  export class Append {
    static readonly type = '[Guess Component] Guess.Append';
    constructor(public value: string) {}
  }

  export class Backspace {
    static readonly type = '[Guess Component] Guess.Backspace';
  }

  export class Submit {
    static readonly type = '[Guess Component] Guess.Submit';
  }

  export class Succeeded {
    static readonly type = '[Guess Component] Guess.Succeeded';
    constructor(public points: number) {}
  }

  export class Failed {
    static readonly type = '[Guess Component] Guess.Failed';
  }
}
