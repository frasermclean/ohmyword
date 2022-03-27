import { Component, OnInit } from '@angular/core';

import { GameService } from 'src/app/services/game.service';

export interface LetterData {
  value: string;
  position: number;
}

@Component({
  selector: 'app-hint',
  templateUrl: './hint.component.html',
  styleUrls: ['./hint.component.scss'],
})
export class HintComponent implements OnInit {
  public hint$ = this.gameService.hint$;
  public letters: LetterData[] = [];

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.getHint();
    this.hint$.subscribe((hint) => {
      this.letters = [];
      for (let i = 0; i < hint.length; i++) {
        this.letters.push({
          position: i + 1,
          value: '',
        });
      }
    });
  }
}
