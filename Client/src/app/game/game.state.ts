import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { GameService } from '../services/game.service';

export interface GameStateModel {
  connected: boolean;
  roundNumber: number;
}

export class Connect {
  static readonly type = '[Game] Connect';
}

export class Disconnect {
  static readonly type = '[Game] Disconnect';
}

const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

@State<GameStateModel>({
  name: GAME_STATE_TOKEN,
  defaults: {
    connected: false,
    roundNumber: 23,
  },
})
@Injectable()
export class GameState {
  constructor(private gameService: GameService) {}

  @Action(Connect)
  connect(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.setState({
      ...state,
      connected: true,
    });
  }

  @Action(Disconnect)
  disconnect(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.setState({
      ...state,
      connected: false,
    });
  }

  @Selector([GAME_STATE_TOKEN])
  static connected(state: GameStateModel) {
    return state.connected;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundNumber(state: GameStateModel) {
    return state.roundNumber;
  }
}


