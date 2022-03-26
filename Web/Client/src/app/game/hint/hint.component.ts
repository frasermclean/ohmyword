import { Component, OnInit } from '@angular/core';

import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-hint',
  templateUrl: './hint.component.html',
  styleUrls: ['./hint.component.scss'],
})
export class HintComponent implements OnInit {
  public hint$ = this.gameService.hint$;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.getHint();
  }
}
