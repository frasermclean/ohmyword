import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameService } from 'src/app/services/game.service';
import { GameState } from '../game.state';
import { Hub, HubState } from '../hub.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.store.select(GameState.registered);
  roundActive$ = this.store.select(GameState.roundActive);
  expiry$ = this.store.select(GameState.expiry);
  connectionState$ = this.store.select(HubState.connectionState);
  roundNumber$ = this.store.select(GameState.roundNumber);

  constructor(private gameService: GameService, private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Hub.Connect());
  }

  ngOnDestroy(): void {
    this.store.dispatch(new Hub.Disconnect());
  }
}
