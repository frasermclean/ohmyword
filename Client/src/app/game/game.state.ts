import { Injectable } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { WordHint } from '../models/word-hint';
import { GameService } from '../services/game.service';
import { Game, Guess, Hub } from './game.actions';

interface GameStateModel {
  registered: boolean;
  playerId: string;
  playerCount: number;
  roundActive: boolean;
  roundNumber: number;
  expiry: Date;
  wordHint: WordHint;
  hub: {
    connectionState: HubConnectionState;
    error: any;
  };
  guess: {
    value: string;
    count: number;
    maxLength: number;
  };
}

const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');
export const GUESS_DEFAULT_CHAR = '_';

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
    hub: {
      connectionState: HubConnectionState.Disconnected,
      error: null,
    },
    guess: {
      value: '',
      count: 0,
      maxLength: 0,
    },
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
      guess: {
        value: '',
        count: 0,
        maxLength: action.wordHint.length,
      },
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
    context.patchState({
      wordHint: {
        ...state.wordHint,
        letterHints,
      },
    });
  }

  @Action(Hub.Connect)
  connect(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.patchState({
      hub: {
        ...state.hub,
        connectionState: HubConnectionState.Connecting,
      },
    });
    this.gameService.connect();
  }

  @Action(Hub.Disconnect)
  disconnect(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.patchState({
      hub: {
        ...state.hub,
        connectionState: HubConnectionState.Disconnecting,
      },
    });
    this.gameService.disconnect();
  }

  @Action(Hub.Connected)
  connected(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.patchState({
      hub: {
        ...state.hub,
        connectionState: HubConnectionState.Connected,
      },
    });
    this.gameService.registerPlayer();
  }

  @Action(Hub.Disconnected)
  disconnected(context: StateContext<GameStateModel>, action: Hub.Disconnected) {
    const state = context.getState();
    context.patchState({
      registered: false,
      playerId: '',
      hub: {
        connectionState: HubConnectionState.Disconnected,
        error: action.error,
      },
    });
  }

  @Action(Hub.ConnectFailed)
  connectionError(context: StateContext<GameStateModel>, action: Hub.ConnectFailed) {
    const state = context.getState();
    context.patchState({
      hub: {
        connectionState: HubConnectionState.Disconnected,
        error: action.error,
      },
    });
  }

  @Action(Guess.Append)
  append(context: StateContext<GameStateModel>, action: Guess.Append) {
    const state = context.getState();
    if (state.guess.value.length === state.guess.maxLength) return;
    context.patchState({
      guess: {
        ...state.guess,
        value: state.guess.value + action.value,
      },
    });
  }

  @Action(Guess.Backspace)
  backspace(context: StateContext<GameStateModel>) {
    const state = context.getState();
    if (state.guess.value.length === 0) return;
    context.patchState({
      guess: {
        ...state.guess,
        value: state.guess.value.slice(0, -1),
      },
    });
  }

  @Action(Guess.Submit)
  submit(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.patchState({
      guess: {
        ...state.guess,
        value: '',
        count: state.guess.count + 1,
      },
    });
  }

  @Selector([GAME_STATE_TOKEN])
  static connectionState(state: GameStateModel) {
    return state.hub.connectionState;
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

  @Selector([GAME_STATE_TOKEN])
  static guessChar(state: GameStateModel) {
    return (index: number) => state.guess.value[index] || GUESS_DEFAULT_CHAR;
  }
}
