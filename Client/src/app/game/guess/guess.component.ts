import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { GuessResponse } from 'src/app/models/guess.response';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit {
  guess = new FormControl('');
  response: GuessResponse | null = null;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {}

  async onGuess(value: string) {
    this.response = await this.gameService.guessWord(value);
    this.guess.reset();
    setTimeout(() => {
      this.response = null;
    }, 2000);
  }
}
