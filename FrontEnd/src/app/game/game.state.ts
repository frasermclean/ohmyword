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
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  interval: {
    startDate: Date;
    endDate: Date;
  };
  wordHint: WordHint;
  score: number;
  guessedWord: boolean;
  roundEndSummary: RoundEndSummary;
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
    roundActive: false,
    roundNumber: 0,
    roundId: '',
    interval: {
      startDate: new Date(),
      endDate: new Date(),
    },
    wordHint: WordHint.default,
    score: 0,
    guessedWord: false,
    roundEndSummary: RoundEndSummary.default,
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
    context.patchState({
      registered: true,
      playerCount: action.playerCount,
      score: action.score,
      roundActive: action.gameState.roundActive,
      roundNumber: action.gameState.roundNumber,
      roundId: action.gameState.roundId,
      wordHint: new WordHint(action.gameState.wordHint),
      interval: {
        startDate: new Date(action.gameState.intervalStart),
        endDate: new Date(action.gameState.intervalEnd),
      },
    });
  }

  @Action(Game.GameStateUpdated)
  gameStateUpdated(context: StateContext<GameStateModel>, action: Game.GameStateUpdated) {
    context.patchState({
      roundActive: action.roundActive,
      roundNumber: action.roundNumber,
      roundId: action.roundId,
      wordHint: action.wordHint,
      interval: {
        startDate: new Date(action.intervalStart),
        endDate: new Date(action.intervalEnd),
      },
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
    this.gameService.registerVisitor();
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

    this.gameService.submitGuess(state.roundId, state.guess.value);
  }

  @Action(Guess.Succeeded)
  guessSucceeded(context: StateContext<GameStateModel>, action: Guess.Succeeded) {
    const state = context.getState();
    context.patchState({
      score: state.score + action.points,
      guessedWord: true,
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
    return state.roundActive;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundNumber(state: GameStateModel) {
    return state.roundNumber;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundId(state: GameStateModel) {
    return state.roundId;
  }

  @Selector([GAME_STATE_TOKEN])
  static interval(state: GameStateModel) {
    return state.interval;
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
  static guessedWord(state: GameStateModel) {
    return state.guessedWord;
  }

  @Selector([GAME_STATE_TOKEN])
  static playerCount(state: GameStateModel) {
    return state.playerCount;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundEndSummary(state: GameStateModel) {
    return state.roundEndSummary;
  }

  @Selector([GAME_STATE_TOKEN])
  static guessChar(state: GameStateModel) {
    return (index: number) => state.guess.value[index] || GUESS_DEFAULT_CHAR;
  }
}
