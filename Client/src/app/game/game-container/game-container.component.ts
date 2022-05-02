import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { GameService } from 'src/app/services/game.service';
import { Connect, Disconnect, GameState } from '../game.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.gameService.registered$;
  roundActive$ = this.gameService.roundActive$;
  expiry$ = this.gameService.expiry$;
  connected$ = this.store.select(GameState.connected);
  roundNumber$ = this.store.select(GameState.roundNumber);

  constructor(private gameService: GameService, private store: Store) {}

  ngOnInit(): void {
    this.gameService.registerPlayer();

    setTimeout(() => {
      this.store.dispatch(new Connect());
    }, 1000);
  }

  ngOnDestroy(): void {
    this.gameService.disconnectAndReset();
    this.store.dispatch(new Disconnect());
  }
}
