import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { Hub } from '../game.actions';
import { GameState } from '../game.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.store.select(GameState.registered);
  roundActive$ = this.store.select(GameState.roundActive);
  expiry$ = this.store.select(GameState.expiry);
  connectionState$ = this.store.select(GameState.connectionState);
  roundNumber$ = this.store.select(GameState.roundNumber);
  score$ = this.store.select(GameState.score);

  constructor(private store: Store) {}

  ngOnInit(): void {
    this.store.dispatch(new Hub.Connect());
  }

  ngOnDestroy(): void {
    this.store.dispatch(new Hub.Disconnect());
  }
}
