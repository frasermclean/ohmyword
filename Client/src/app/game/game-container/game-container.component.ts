import { Component, OnDestroy, OnInit } from '@angular/core';
import { Store } from '@ngxs/store';
import { tap } from 'rxjs/operators';
import { GameService } from 'src/app/services/game.service';
import { GameState, Hub } from '../game.state';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.gameService.registered$;
  roundActive$ = this.gameService.roundActive$;
  expiry$ = this.gameService.expiry$;
  hubConnection$ = this.store.select(GameState.hubConnection);
  roundNumber$ = this.store.select(GameState.roundNumber);

  constructor(private gameService: GameService, private store: Store) {}

  ngOnInit(): void {
    //this.gameService.registerPlayer();
    this.store.dispatch(new Hub.Connect());

    this.hubConnection$.pipe(tap(value => console.log('hubConnection', value))).subscribe();
  }

  ngOnDestroy(): void {
    this.gameService.disconnectAndReset();
    this.store.dispatch(new Hub.Disconnect());
  }
}
