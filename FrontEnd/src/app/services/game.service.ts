import { Injectable } from '@angular/core';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';

import { environment } from 'src/environments/environment';
import { FingerprintService } from './fingerprint.service';

import { RegisterVisitorResponse } from '../models/responses/register-visitor.response';
import { GuessResponse } from '../models/responses/guess.response';
import { LetterHintResponse } from '../models/responses/letter-hint.response';
import { RoundStartResponse } from '../models/responses/round-start.response';
import { RoundEndResponse } from '../models/responses/round-end.response';

import { Store } from '@ngxs/store';

import { Game, Guess, Hub } from '../game/game.actions';

@Injectable({
  providedIn: 'root',
})
export class GameService {
  private readonly hubConnection = new HubConnectionBuilder()
    .withUrl(`https://${environment.apiHost}/hub`)
    .configureLogging(environment.name !== 'development' ? LogLevel.Error : LogLevel.Information)
    .build();

  constructor(private fingerprintService: FingerprintService, private store: Store) {
    // register callbacks
    this.hubConnection.onclose((error) => this.store.dispatch(new Hub.Disconnected(error)));
    this.hubConnection.on('SendRoundStarted', (response: RoundStartResponse) =>
      this.store.dispatch(new Game.RoundStarted(response))
    );
    this.hubConnection.on('SendRoundEnded', (response: RoundEndResponse) =>
      this.store.dispatch(new Game.RoundEnded(response))
    );
    this.hubConnection.on('SendLetterHint', (response: LetterHintResponse) => {
      this.store.dispatch(new Game.LetterHintReceived(response));
    });
    this.hubConnection.on('SendPlayerCount', (count: number) =>
      this.store.dispatch(new Game.PlayerCountUpdated(count))
    );
  }

  /**
   * Connect to game hub (if not already connected)
   */
  public async connect() {
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      console.warn('Invalid hub connection state to perform connect:', this.hubConnection.state);
      return;
    }

    try {
      await this.hubConnection.start();
      this.store.dispatch(new Hub.Connected());
    } catch (error) {
      this.store.dispatch(new Hub.ConnectFailed(error));
    }
  }

  /**
   * Disconnect from game hub
   */
  public disconnect() {
    if (this.hubConnection.state !== HubConnectionState.Connected) {
      console.warn('Invalid hub connection state to perform disconnect:', this.hubConnection.state);
      return;
    }

    this.hubConnection.stop();
  }

  /**
   * Attempt to register with game service.
   */
  public async registerVisitor() {
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterVisitorResponse>('RegisterVisitor', visitorId);
    this.store.dispatch(new Game.PlayerRegistered(response));
    if (response.roundStart) this.store.dispatch(new Game.RoundStarted(response.roundStart));
    else if (response.roundEnd) this.store.dispatch(new Game.RoundEnded(response.roundEnd));
  }

  /**
   * Submit to word guess to be evaluated by the game server.
   * @param roundId The current round ID.
   * @param value The value of the guess to submit.
   */
  public async submitGuess(roundId: string, value: string) {
    const response = await this.hubConnection.invoke<GuessResponse>('SubmitGuess', roundId, value);
    this.store.dispatch(response.correct ? new Guess.Succeeded(response.points) : new Guess.Failed());
  }
}
