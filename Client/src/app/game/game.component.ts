import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameComponent implements OnInit {
  status$ = this.gameService.status$;
  hint$ = this.gameService.hint$;
  guess = '';

  constructor(private gameService: GameService) {}

  ngOnInit() {
    this.gameService.registerPlayer();
  }

  onGuessChanged(value: string) {
    this.guess = value;
  }
}
