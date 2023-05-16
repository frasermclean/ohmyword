import { GameStateResponse } from '@models/responses/game-state.response';
import { LetterHintResponse } from '@models/responses/letter-hint.response';
import { RegisterPlayerResponse } from '@models/responses/register-player.response';
import { WordHint } from '@models/word-hint.model';
import { RoundEndedResponse } from "@models/responses/round-ended.response";
import { RoundStartedResponse } from "@models/responses/round-started.response";

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

    roundNumber = this.response.roundNumber;
    roundId = this.response.roundId;
    startDate = new Date(this.response.startDate);
    endDate = new Date(this.response.endDate);
    wordHint = new WordHint(this.response.wordHint);

    constructor(private response: RoundStartedResponse) {}
  }

  export class RoundEnded {
    static readonly type = '[Game Service] Game.RoundEnded';

    word = this.response.word;
    partOfSpeech = this.response.partOfSpeech;
    endReason = this.response.endReason;
    nextRoundStart = new Date(this.response.nextRoundStart);
    scores = this.response.scores;

    constructor(private response: RoundEndedResponse) {}
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
