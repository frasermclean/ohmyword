import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { HintComponent } from './hint/hint.component';
import { GuessComponent } from './guess/guess.component';
import { GameComponent } from './game.component';
import { LetterComponent } from './hint/letter/letter.component';
import { CountdownComponent } from './countdown/countdown.component';

@NgModule({
  declarations: [HintComponent, GuessComponent, GameComponent, LetterComponent, CountdownComponent],
  imports: [CommonModule, ReactiveFormsModule],
  exports: [GameComponent],
})
export class GameModule {}
