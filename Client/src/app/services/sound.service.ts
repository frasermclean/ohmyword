import { Injectable } from '@angular/core';
import { Howl } from 'howler';

export enum SoundSprite {
  Correct = 'correct',
  Incorrect = 'incorrect',
}

@Injectable({
  providedIn: 'root',
})
export class SoundService {
  private howl = new Howl({
    src: 'assets/audio/sprites.ogg',
    sprite: {
      correct: [0, 720],
      incorrect: [721, 810],
    },
    autoplay: false,
    preload: true,
  });

  private play(sprite: SoundSprite) {
    this.howl.play(sprite);
  }

  public playCorrect() {
    this.play(SoundSprite.Correct);
  }

  public playIncorrect() {
    this.play(SoundSprite.Incorrect);
  }
}
