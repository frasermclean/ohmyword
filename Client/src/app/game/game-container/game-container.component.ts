import { Component, OnDestroy, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'game-container',
  templateUrl: './game-container.component.html',
  styleUrls: ['./game-container.component.scss'],
})
export class GameContainerComponent implements OnInit, OnDestroy {
  registered$ = this.gameService.registered$;
  roundActive$ = this.gameService.roundActive$;
  expiry$ = this.gameService.expiry$;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.registerPlayer();
  }

  ngOnDestroy(): void {
    this.gameService.disconnectAndReset();
  }
}
