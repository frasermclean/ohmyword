import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameComponent implements OnInit {
  registered$ = this.gameService.registered$;
  roundActive$ = this.gameService.roundActive$;
  expiry$ = this.gameService.expiry$;

  constructor(private gameService: GameService) {}

  ngOnInit() {
    this.gameService.registerPlayer();
  }
}
