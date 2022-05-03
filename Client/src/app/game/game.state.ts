import { Injectable } from '@angular/core';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { tap } from 'rxjs/operators';
import { GameService } from '../services/game.service';

export interface GameStateModel {
  hubConnection: 'initial' | 'connecting' | 'connected' | 'disconnected';
  roundNumber: number;
}

export namespace Hub {
  export class Connect {
    static readonly type = '[Game Container] Hub.Connect';
  }

  export class Disconnect {
    static readonly type = '[Game Container] Hub.Disconnect';
  }

  export class Disconnected {
    static readonly type = '[Game Service] Hub.Disconnected';
    constructor(public error?: Error) {}
  }
}

const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

@State<GameStateModel>({
  name: GAME_STATE_TOKEN,
  defaults: {
    hubConnection: 'initial',
    roundNumber: 0,
  },
})
@Injectable()
export class GameState {
  constructor(private gameService: GameService) {}

  @Action(Hub.Connect)
  async connect(context: StateContext<GameStateModel>) {
    let state = context.getState();
    context.patchState({ ...state, hubConnection: 'connecting' });

    try {
      await this.gameService.connect();
      state = context.getState();
      context.patchState({ ...state, hubConnection: 'connected' });
    } catch (error) {
      state = context.getState();
      context.patchState({ ...state, hubConnection: 'disconnected' });
      throw error;
    }
  }

  @Action(Hub.Disconnect)
  disconnect() {
    this.gameService.disconnect();
  }

  @Action(Hub.Disconnected)
  disconnected(context: StateContext<GameStateModel>) {
    const state = context.getState();
    context.setState({
      ...state,
      hubConnection: 'disconnected',
    });
  }

  @Selector([GAME_STATE_TOKEN])
  static hubConnection(state: GameStateModel) {
    return state.hubConnection;
  }

  @Selector([GAME_STATE_TOKEN])
  static roundNumber(state: GameStateModel) {
    return state.roundNumber;
  }
}
