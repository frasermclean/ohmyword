import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-game',
  templateUrl: './game.component.html',
  styleUrls: ['./game.component.scss'],
})
export class GameComponent implements OnInit {
  isRegistered$ = this.gameService.isRegistered$;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.registerPlayer();
  }
}
