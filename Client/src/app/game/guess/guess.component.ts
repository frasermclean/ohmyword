import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';
import { Howl } from 'howler';
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
  sound = new Howl({
    src: 'assets/audio/sprites.ogg',
    sprite: {
      correct: [0, 720],
      incorrect: [721, 810],
    },
    autoplay: false,
    preload: true,
  });

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    console.log(this.sound);
  }

  async onGuess(value: string) {
    this.response = await this.gameService.guessWord(value);

    // play sound
    const soundId = this.response.correct ? 'correct' : 'incorrect';
    this.sound.play(soundId);

    this.guess.reset();
    setTimeout(() => {
      this.response = null;
    }, 2000);
  }
}
