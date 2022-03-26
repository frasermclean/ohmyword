import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';

import { HintComponent } from './hint/hint.component';
import { GuessComponent } from './guess/guess.component';

@NgModule({
  declarations: [HintComponent, GuessComponent],
  imports: [CommonModule, ReactiveFormsModule],
  exports: [HintComponent, GuessComponent],
})
export class GameModule {}
