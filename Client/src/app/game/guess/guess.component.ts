import { Component, EventEmitter, HostListener, OnInit, Output } from '@angular/core';
import { GameService } from 'src/app/services/game.service';
import { GuessResponse } from 'src/app/models/responses/guess.response';

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit {
  response: GuessResponse | null = null;

  @Output()
  valueChanged = new EventEmitter<string>();

  @HostListener('window:keyup', ['$event'])
  onKeyUpEvent(event: KeyboardEvent) {
    switch (event.key) {
      case 'Backspace':
        this.deleteCharFromGuess();
        break;
      case 'Enter':
        this.submitGuess();
        break;
      default:
        if (event.key.length !== 1 || !event.key[0].match(/[A-z]/g)) return;
        this.addCharToGuess(event.key.toLowerCase());
        break;
    }
  }

  constructor(private gameService: GameService) {}

  ngOnInit(): void {}

  addCharToGuess(char: string) {
    this.gameService.guess += char;
  }

  deleteCharFromGuess() {
    if (this.gameService.guess.length === 0) return;
    this.gameService.guess = this.gameService.guess.slice(0, -1);
  }

  submitGuess() {
    this.gameService.submitGuess();
  }
}
