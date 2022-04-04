import { Component, OnInit } from '@angular/core';
import { HintResponse } from 'src/app/models/hint.response';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameComponent implements OnInit {
  registered$ = this.gameService.registered$;
  hint$ = this.gameService.hint$;
  guess = '';

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.registerPlayer();
  }

  onGuessChanged(value: string) {
    this.guess = value;
  }
}
