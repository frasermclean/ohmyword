import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameComponent implements OnInit {
  registered$ = this.gameService.registered$;
  status$ = this.gameService.gameStatus$;
  hint$ = this.gameService.wordHint$;

  constructor(private gameService: GameService) {}

  ngOnInit() {
    this.gameService.registerPlayer();
  }
}
