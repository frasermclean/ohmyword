import { Injectable } from '@angular/core';
import { HubConnectionBuilder, HubConnectionState, LogLevel } from '@microsoft/signalr';
import { Store } from '@ngxs/store';
import { environment } from '@environment';
import { FingerprintService } from '@services/fingerprint.service';
import { AuthService } from '@services/auth.service';

import { Game } from '@state/game/game.actions';
import { Guess } from '@state/guess/guess.actions';
import { Hub } from '@state/hub/hub.actions';
import { RegisterPlayerResponse } from '@models/responses/register-player.response';
import { SubmitGuessResult } from '@models/submit-guess-result.model';
import { LetterHintResponse } from '@models/responses/letter-hint.response';
import { RoundEndedModel } from "@models/round-ended.model";
import { RoundStartedModel } from "@models/round-started.model";

@Injectable({
  providedIn: 'root',
})
export class HubService {
  private readonly hubConnection = new HubConnectionBuilder()
    .withUrl(`https://${environment.apiHost}/hub`, {
      accessTokenFactory: () => this.authService.getApiAccessToken(),
    })
    .configureLogging(environment.name !== 'development' ? LogLevel.Error : LogLevel.Information)
    .build();

  constructor(private fingerprintService: FingerprintService, private store: Store, private authService: AuthService) {
    this.registerHubCallbacks();
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
      this.store.dispatch(new Hub.Connected(this.hubConnection.connectionId));
    } catch (error) {
      this.store.dispatch(new Hub.Disconnected(error));
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
  public async registerPlayer() {
    const visitorId = await this.fingerprintService.getVisitorId();
    const response = await this.hubConnection.invoke<RegisterPlayerResponse>(this.registerPlayer.name, visitorId);
    this.store.dispatch(new Game.PlayerRegistered(response));
  }

  /**
   * Submit to word guess to be evaluated by the game server.
   * @param roundId The current round ID.
   * @param value The value of the guess to submit.
   */
  public async submitGuess(roundId: string, value: string) {
    const result = await this.hubConnection.invoke<SubmitGuessResult>(this.submitGuess.name, roundId, value);
    this.store.dispatch(result.isCorrect ? new Guess.Succeeded(result.pointsAwarded) : new Guess.Failed());
  }

  /**
   * Register hub connection callback methods.
   */
  private registerHubCallbacks() {
    // connect closed
    this.hubConnection.onclose((error) => this.store.dispatch(new Hub.Disconnected(error)));

    // server sent game state
    this.hubConnection.on('SendRoundStarted', (response: RoundStartedModel) =>
      this.store.dispatch(new Game.RoundStarted(response))
    );

    // round ended
    this.hubConnection.on('SendRoundEnded', (data: RoundEndedModel) =>
      this.store.dispatch(new Game.RoundEnded(data))
    );

    // player count changed
    this.hubConnection.on('SendPlayerCount', (count: number) =>
      this.store.dispatch(new Game.PlayerCountUpdated(count))
    );

    // letter hint received
    this.hubConnection.on('SendLetterHint', (response: LetterHintResponse) => {
      this.store.dispatch(new Game.LetterHintReceived(response));
    });
  }
}
