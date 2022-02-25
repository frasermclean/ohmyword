import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HintComponent } from './hint/hint.component';

@NgModule({
  declarations: [HintComponent],
  imports: [CommonModule],
  exports: [HintComponent],
})
export class GameModule {}
