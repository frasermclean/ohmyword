import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { RoundSummary } from '@models/round-summary.model';
import { WordHint } from '@models/word-hint.model';
import { HubService } from '@services/hub.service';
import { Game } from './game.actions';
import { Guess } from '../guess/guess.actions';
import { Hub } from '@state/hub/hub.actions';

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
  wordHint: WordHint | null;
  score: number;
  roundSummary: RoundSummary | null;
}

export const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

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
    wordHint: null,
    score: 0,
    roundSummary: null,
  },
})
@Injectable()
export class GameState {
  constructor(private hubService: HubService) {}

  @Action(Game.RegisterPlayer)
  registerPlayer(context: StateContext<GameStateModel>) {
    this.hubService.registerPlayer();
  }

  @Action(Game.PlayerRegistered)
  registered(context: StateContext<GameStateModel>, action: Game.PlayerRegistered) {
    context.patchState({
      registered: true,
      playerCount: action.playerCount,
      score: action.score,
      roundActive: action.gameState.roundActive,
      roundNumber: action.gameState.roundNumber,
      roundId: action.gameState.roundId,
      wordHint: action.gameState.wordHint ? new WordHint(action.gameState.wordHint) : null,
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
      roundSummary: action.roundSummary,
      interval: {
        startDate: new Date(action.intervalStart),
        endDate: new Date(action.intervalEnd),
      },
    });

    // reset guess on new round
    if (action.roundActive) {
      context.dispatch(new Guess.SetNewWord(action.wordHint));
    }
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

  @Action(Hub.Disconnected)
  hubDisconnected(context: StateContext<GameStateModel>) {
    context.patchState({
      registered: false,
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