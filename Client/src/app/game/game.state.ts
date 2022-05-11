import { Injectable } from '@angular/core';
import { HubConnectionState } from '@microsoft/signalr';
import { Action, Selector, State, StateContext, StateToken } from '@ngxs/store';
import { RegisterPlayerResponse } from '../models/responses/register-player.response';
import { RoundEndResponse } from '../models/responses/round-end.response';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { GameService } from '../services/game.service';

export interface GameStateModel {
  hubConnection: HubConnectionState;
  registered: boolean;
  playerId: string;
  playerCount: number;
  roundActive: boolean;
  roundNumber: number;
  expiry: Date;
  error: any;
}

export namespace GameHub {
  export class Connect {
    static readonly type = '[Game Container] GameHub.Connect';
  }

  export class Disconnect {
    static readonly type = '[Game Container] GameHub.Disconnect';
  }

  export class Connected {
    static readonly type = '[Game Service] GameHub.Connected';
  }

  export class Disconnected {
    static readonly type = '[Game Service] GameHub.Disconnected';
    constructor(public error?: Error) {}
  }

  export class ConnectFailed {
    static readonly type = '[Game Service] GameHub.ConnectFailed';
    constructor(public error?: any) {}
  }
}

export namespace Player {
  export class Registered {
    static readonly type = '[Game Service] Player.Registered';
    constructor(public response: RegisterPlayerResponse) {}
  }
}

export namespace Round {
  export class Started implements RoundStartResponse {
    static readonly type = '[Game Service] Round.Started';

    roundId = this.response.roundId;
    roundNumber = this.response.roundNumber;
    roundEnds = this.response.roundEnds;
    wordHint = this.response.wordHint;

    constructor(private response: RoundStartResponse) {}
  }

  export class Ended  {
    static readonly type = '[Game Service] Round.Ended';

    roundId = this.response.roundId;
    nextRoundStart = new Date(this.response.nextRoundStart);
    word = this.response.word;

    constructor(private response: RoundEndResponse) {}
  }
}


const GAME_STATE_TOKEN = new StateToken<GameStateModel>('game');

@State<GameStateModel>({
  name: GAME_STATE_TOKEN,
  defaults: {
    hubConnection: HubConnectionState.Disconnected,
    registered: false,
    playerId: '',
    playerCount: 0,
    roundActive: false,
    roundNumber: 0,
    expiry: new Date(),
    error: null,
  },
})
@Injectable()
export class GameState {
  constructor(private gameService: GameService) {}

  @Action(GameHub.Connect)
  connect(context: StateContext<GameStateModel>) {
    context.patchState({ hubConnection: HubConnectionState.Connecting });
    this.gameService.connect();
  }

  @Action(GameHub.Disconnect)
  disconnect(context: StateContext<GameStateModel>) {
    context.patchState({ hubConnection: HubConnectionState.Disconnecting });
    this.gameService.disconnect();
  }

  @Action(GameHub.Connected)
  connected(context: StateContext<GameStateModel>) {
    context.patchState({ hubConnection: HubConnectionState.Connected });
    this.gameService.registerPlayer();
  }

  @Action(GameHub.Disconnected)
  disconnected(context: StateContext<GameStateModel>, action: GameHub.Disconnected) {
    context.patchState({
      hubConnection: HubConnectionState.Disconnected,
      registered: false,
      error: action.error,
    });
  }

  @Action(GameHub.ConnectFailed)
  connectionError(context: StateContext<GameStateModel>, action: GameHub.ConnectFailed) {
    context.patchState({
      hubConnection: HubConnectionState.Disconnected,
      error: action.error,
      registered: false,
    });
  }

  @Action(Player.Registered)
  registered(context: StateContext<GameStateModel>, action: Player.Registered) {
    context.patchState({
      registered: true,
      playerId: action.response.playerId,
      playerCount: action.response.playerCount,
      roundActive: action.response.roundActive,
      roundNumber: action.response.roundNumber,
      expiry: new Date(action.response.expiry),
    });
  }

  @Action(Round.Started)
  roundStarted(context: StateContext<GameStateModel>, action: Round.Started) {
    context.patchState({
      roundActive: true,
      roundNumber: action.roundNumber,
      expiry: new Date(action.roundEnds),
    })
  }

  @Action(Round.Ended)
  roundEnded(context: StateContext<GameStateModel>, action: Round.Ended) {
    context.patchState({
      roundActive: false,
      expiry: new Date(action.nextRoundStart),
    })
  }

  @Selector([GAME_STATE_TOKEN])
  static hubConnection(state: GameStateModel) {
    return state.hubConnection;
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
}
