import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';

import { Hub } from './hub.actions';
import { HubService } from '@services/hub.service';
import { Injectable } from '@angular/core';
import { Game } from '@state/game/game.actions';

interface HubStateModel {
  connectionState: 'disconnected' | 'connecting' | 'connected' | 'disconnecting';
  connectionId: string | null;
  error: any;
}

export const HUB_STATE_TOKEN = new StateToken<HubStateModel>('hub');

@State<HubStateModel>({
  name: HUB_STATE_TOKEN,
  defaults: {
    connectionState: 'disconnected',
    connectionId: null,
    error: null,
  },
})
@Injectable()
export class HubState {
  constructor(private hubService: HubService) {}

  @Action(Hub.Connect)
  connect(context: StateContext<HubStateModel>) {
    context.patchState({
      connectionState: 'connecting',
    });
    this.hubService.connect();
  }

  @Action(Hub.Disconnect)
  disconnect(context: StateContext<HubStateModel>, action: Hub.Disconnect) {
    context.patchState({
      connectionState: 'disconnecting',
    });
    this.hubService.disconnect();
  }

  @Action(Hub.Connected)
  connected(context: StateContext<HubStateModel>, action: Hub.Connected) {
    context.patchState({
      connectionState: 'connected',
      connectionId: action.connectionId,
      error: null,
    });
    context.dispatch(new Game.RegisterPlayer());
  }

  @Action(Hub.Disconnected)
  connectionError(context: StateContext<HubStateModel>, action: Hub.Disconnected) {
    context.patchState({
      connectionState: 'disconnected',
      connectionId: null,
      error: action.error,
    });
  }

  @Selector([HUB_STATE_TOKEN])
  static connectionState(state: HubStateModel) {
    return state.connectionState;
  }
}
