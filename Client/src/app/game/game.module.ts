import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { HintComponent } from './hint/hint.component';
import { GuessComponent } from './guess/guess.component';
import { GameComponent } from './game/game.component';

@NgModule({
  declarations: [HintComponent, GuessComponent, GameComponent],
  imports: [CommonModule, ReactiveFormsModule],
  exports: [GameComponent],
})
export class GameModule {}
