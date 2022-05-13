import { Injectable } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { RoundEndSummary } from '../models/round-end-summary.model';
import { WordHint } from '../models/word-hint.model';
import { GameService } from '../services/game.service';
import { SoundService } from '../services/sound.service';
import { Game, Guess, Hub } from './game.actions';

interface GameStateModel {
  registered: boolean;
  playerCount: number;
  round: {
    active: boolean;
    number: number;
    id: string;
    guessed: boolean;
    endSummary: RoundEndSummary | null;
  };
  expiry: Date;
  wordHint: WordHint;
  score: number;
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
    playerCount: 0,
    round: {
      active: false,
      number: 0,
      id: '',
      guessed: false,
      endSummary: null,
    },
    expiry: new Date(),
    wordHint: WordHint.default,
    score: 0,
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
  constructor(private gameService: GameService, private soundService: SoundService) {}

  @Action(Game.PlayerRegistered)
  registered(context: StateContext<GameStateModel>, action: Game.PlayerRegistered) {
    const state = context.getState();
    context.patchState({
      registered: true,
      playerCount: action.playerCount,
      round: {
        ...state.round,
        active: action.roundActive,
        number: action.roundNumber,
        id: action.roundId,
      },
      expiry: action.expiry,
      wordHint: action.wordHint,
      score: action.score,
    });
  }

  @Action(Game.RoundStarted)
  roundStarted(context: StateContext<GameStateModel>, action: Game.RoundStarted) {
    context.patchState({
      round: {
        active: true,
        number: action.roundNumber,
        id: action.roundId,
        guessed: false,
        endSummary: null,
      },
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
    const state = context.getState();
    context.patchState({
      round: {
        ...state.round,
        active: false,
        id: '',
        endSummary: new RoundEndSummary(action.word, action.endReason),
      },
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

  @Action(Game.PlayerCountUpdated)
  playerCountUpdated(context: StateContext<GameStateModel>, action: Game.PlayerCountUpdated) {
    context.patchState({ playerCount: action.count });
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
    context.patchState({
      registered: false,
      hub: {
        connectionState: HubConnectionState.Disconnected,
        error: action.error,
      },
    });
  }

  @Action(Hub.ConnectFailed)
  connectionError(context: StateContext<GameStateModel>, action: Hub.ConnectFailed) {
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

    this.gameService.submitGuess(state.round.id, state.guess.value);
  }

  @Action(Guess.Succeeded)
  guessSucceeded(context: StateContext<GameStateModel>, action: Guess.Succeeded) {
    const state = context.getState();
    context.patchState({
      score: state.score + action.points,
      round: {
        ...state.round,
        guessed: true,
      },
    });
    this.soundService.playCorrect();
  }

  @Action(Guess.Failed)
  guessFailed() {
    this.soundService.playIncorrect();
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
    return state.round.active;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundNumber(state: GameStateModel) {
    return state.round.number;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundEndSummary(state: GameStateModel) {
    return state.round.endSummary;
  }

  @Selector([GAME_STATE_TOKEN])
  static guessed(state: GameStateModel) {
    return state.round.guessed;
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
  static score(state: GameStateModel) {
    return state.score;
  }

  @Selector([GAME_STATE_TOKEN])
  static playerCount(state: GameStateModel) {
    return state.playerCount;
  }

  @Selector([GAME_STATE_TOKEN])
  static guessChar(state: GameStateModel) {
    return (index: number) => state.guess.value[index] || GUESS_DEFAULT_CHAR;
  }
}
