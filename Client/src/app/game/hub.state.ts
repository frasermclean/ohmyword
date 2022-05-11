import { Injectable } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { GameService } from '../services/game.service';

export interface HubStateModel {
  connectionState: HubConnectionState;
  error: any;
}

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

const HUB_STATE_TOKEN = new StateToken<HubStateModel>('hub');

@State<HubStateModel>({
  name: HUB_STATE_TOKEN,
  defaults: {
    connectionState: HubConnectionState.Disconnected,
    error: null,
  },
})
@Injectable()
export class HubState {
  constructor(private gameService: GameService) {}

  @Action(Hub.Connect)
  connect(context: StateContext<HubStateModel>) {
    context.patchState({ connectionState: HubConnectionState.Connecting });
    this.gameService.connect();
  }

  @Action(Hub.Disconnect)
  disconnect(context: StateContext<HubStateModel>) {
    context.patchState({ connectionState: HubConnectionState.Disconnecting });
    this.gameService.disconnect();
  }

  @Action(Hub.Connected)
  connected(context: StateContext<HubStateModel>) {
    context.patchState({ connectionState: HubConnectionState.Connected });
    this.gameService.registerPlayer();
  }

  @Action(Hub.Disconnected)
  disconnected(context: StateContext<HubStateModel>, action: Hub.Disconnected) {
    context.patchState({
      connectionState: HubConnectionState.Disconnected,
      error: action.error,
    });
  }

  @Action(Hub.ConnectFailed)
  connectionError(context: StateContext<HubStateModel>, action: Hub.ConnectFailed) {
    context.patchState({
      connectionState: HubConnectionState.Disconnected,
      error: action.error,
    });
  }

  @Selector([HUB_STATE_TOKEN])
  static connectionState(state: HubStateModel) {
    return state.connectionState;
  }
}
