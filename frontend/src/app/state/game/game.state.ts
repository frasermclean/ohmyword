import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { WordHint } from '@models/word-hint.model';
import { HubService } from '@services/hub.service';
import { Game } from './game.actions';
import { Hub } from '@state/hub/hub.actions';
import { PartOfSpeech } from '@models/enums';
import { RoundSummary } from '@models/round-summary.model';

interface GameStateModel {
  connection: 'disconnected' | 'connecting' | 'connected' | 'registering' | 'registered' | 'disconnecting';
  playerCount: number;
  roundActive: boolean;
  roundNumber: number;
  roundId: string;
  score: number;
  interval: {
    startDate: Date;
    endDate: Date;
  };
  wordHint: WordHint | null;
  roundSummary: RoundSummary | null;
}

export const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

@State<GameStateModel>({
  name: GAME_STATE_TOKEN,
  defaults: {
    connection: 'disconnected',
    playerCount: 0,
    roundActive: false,
    roundNumber: 0,
    roundId: '',
    score: 0,
    interval: {
      startDate: new Date(),
      endDate: new Date(),
    },
    wordHint: null,
    roundSummary: null,
  },
})
@Injectable()
export class GameState {
  constructor(private hubService: HubService) {}

  @Action(Game.RegisterPlayer)
  registerPlayer(context: StateContext<GameStateModel>) {
    context.patchState({
      connection: 'registering',
    });
    this.hubService.registerPlayer();
  }

  @Action(Game.RegisterPlayerSucceeded)
  registerPlayerSucceeded(context: StateContext<GameStateModel>, action: Game.RegisterPlayerSucceeded) {
    context.patchState({
      connection: 'registered',
      playerCount: action.data.playerCount,
      score: action.data.score,
      roundActive: action.data.stateSnapshot.roundActive,
      roundNumber: action.data.stateSnapshot.roundNumber,
      roundId: action.data.stateSnapshot.roundId,
      wordHint: action.data.stateSnapshot.wordHint ? new WordHint(action.data.stateSnapshot.wordHint) : null,
      interval: {
        startDate: new Date(action.data.stateSnapshot.interval.startDate),
        endDate: new Date(action.data.stateSnapshot.interval.endDate),
      },
    });
  }

  @Action(Game.RegisterPlayerFailed)
  registerPlayerFailed(context: StateContext<GameStateModel>, action: Game.RegisterPlayerFailed) {
    if (action.error) {
      console.error('Failed to register player.', action.error);
    }
    context.dispatch(new Hub.Disconnect());
  }

  @Action(Game.RoundStarted)
  gameStateUpdated(context: StateContext<GameStateModel>, action: Game.RoundStarted) {
    context.patchState({
      roundActive: true,
      roundNumber: action.data.roundNumber,
      roundId: action.data.roundId,
      wordHint: new WordHint(action.data.wordHint),
      roundSummary: null,
      interval: {
        startDate: new Date(action.data.startDate),
        endDate: new Date(action.data.endDate),
      },
    });
  }

  @Action(Game.RoundEnded)
  roundEnded(context: StateContext<GameStateModel>, action: Game.RoundEnded) {
    context.patchState({
      roundActive: false,
      roundSummary: action.summary,
      interval: {
        startDate: new Date(),
        endDate: new Date(action.summary.nextRoundStart),
      },
    });
  }

  @Action(Game.LetterHintReceived)
  letterHintReceived(context: StateContext<GameStateModel>, action: Game.LetterHintReceived) {
    const state = context.getState();
    if (!state.wordHint) {
      console.warn('Received letter hint but word hint is null');
      return;
    }
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

  @Action(Game.AddPoints)
  addPoints(context: StateContext<GameStateModel>, action: Game.AddPoints) {
    const currentScore = context.getState().score;
    context.patchState({ score: currentScore + action.points });
  }

  @Action(Hub.Connect)
  hubConnecting(context: StateContext<GameStateModel>) {
    context.patchState({
      connection: 'connecting',
    });
  }

  @Action(Hub.Connected)
  hubConnected(context: StateContext<GameStateModel>) {
    context.patchState({
      connection: 'connected',
    });
  }

  @Action(Hub.Disconnected)
  hubDisconnected(context: StateContext<GameStateModel>) {
    context.patchState({
      connection: 'disconnected',
    });
  }

  @Selector([GAME_STATE_TOKEN])
  static connection(state: GameStateModel) {
    return state.connection;
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
  static playerCount(state: GameStateModel) {
    return state.playerCount;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundSummary(state: GameStateModel) {
    return state.roundSummary;
  }
}
