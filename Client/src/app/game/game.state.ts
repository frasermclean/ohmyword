import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { LetterHintResponse } from '../models/responses/letter-hint.response';
import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { RoundEndResponse } from '../models/responses/round-end.response';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { WordHint } from '../models/word-hint';
import { GameService } from '../services/game.service';
import { Hub } from './hub.state';

export interface GameStateModel {
  registered: boolean;
  playerId: string;
  playerCount: number;
  roundActive: boolean;
  roundNumber: number;
  expiry: Date;
  wordHint: WordHint;
}

export namespace Game {
  export class PlayerRegistered {
    static readonly type = '[Game Service] Game.PlayerRegistered';

    playerId = this.response.playerId;
    playerCount = this.response.playerCount;
    roundActive = this.response.roundActive;
    roundNumber = this.response.roundNumber;
    expiry = new Date(this.response.expiry);
    wordHint = this.response.wordHint ? new WordHint(this.response.wordHint) : WordHint.default;

    constructor(private response: RegisterPlayerResponse) {}
  }

  export class RoundStarted implements RoundStartResponse {
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
}

const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

@State<GameStateModel>({
  name: GAME_STATE_TOKEN,
  defaults: {
    registered: false,
    playerId: '',
    playerCount: 0,
    roundActive: false,
    roundNumber: 0,
    expiry: new Date(),
    wordHint: WordHint.default,
  },
})
@Injectable()
export class GameState {
  constructor(private gameService: GameService) {}

  @Action(Game.PlayerRegistered)
  registered(context: StateContext<GameStateModel>, action: Game.PlayerRegistered) {
    context.patchState({
      registered: true,
      playerId: action.playerId,
      playerCount: action.playerCount,
      roundActive: action.roundActive,
      roundNumber: action.roundNumber,
      expiry: action.expiry,
      wordHint: action.wordHint,
    });
  }

  @Action(Game.RoundStarted)
  roundStarted(context: StateContext<GameStateModel>, action: Game.RoundStarted) {
    context.patchState({
      roundActive: true,
      roundNumber: action.roundNumber,
      expiry: new Date(action.roundEnds),
      wordHint: new WordHint(action.wordHint),
    });
  }

  @Action(Game.RoundEnded)
  roundEnded(context: StateContext<GameStateModel>, action: Game.RoundEnded) {
    context.patchState({
      roundActive: false,
      expiry: new Date(action.nextRoundStart),
      wordHint: WordHint.default,
    });
  }

  @Action(Game.LetterHintReceived)
  letterHintReceived(context: StateContext<GameStateModel>, action: Game.LetterHintReceived) {
    const state = context.getState();
    const letterHints = [...state.wordHint.letterHints];
    letterHints[action.position - 1] = action.value;
    context.setState({
      ...state,
      wordHint: {
        ...state.wordHint,
        letterHints,
      },
    });
  }

  @Action(Hub.Disconnected)
  disconnected(context: StateContext<GameStateModel>) {
    context.patchState({
      registered: false,
      playerId: '',
    });
  }

  @Selector([GAME_STATE_TOKEN])
  static registered(state: GameStateModel) {
    return state.registered;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundActive(state: GameStateModel) {
    return state.roundActive;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundNumber(state: GameStateModel) {
    return state.roundNumber;
  }

  @Selector([GAME_STATE_TOKEN])
  static expiry(state: GameStateModel) {
    return state.expiry;
  }

  @Selector([GAME_STATE_TOKEN])
  static wordHint(state: GameStateModel) {
    return state.wordHint;
  }
}
