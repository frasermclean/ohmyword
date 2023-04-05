import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { NgxsModule } from '@ngxs/store';

// material modules
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';

// states
import { GameState } from '@state/game/game.state';
import { GuessState } from '@state/guess/guess.state';

// components
import { HintComponent } from './hint/hint.component';
import { GuessComponent } from './guess/guess.component';
import { GameContainerComponent } from './game-container/game-container.component';
import { LetterComponent } from './hint/letter/letter.component';
import { CountdownComponent } from './countdown/countdown.component';
import { RoundEndSummaryComponent } from './round-end-summary/round-end-summary.component';
import { KeyboardComponent } from './keyboard/keyboard.component';
import { StatsComponent } from './stats/stats.component';

const routes: Routes = [{ path: '', component: GameContainerComponent }];

@NgModule({
  declarations: [
    HintComponent,
    GuessComponent,
    GameContainerComponent,
    LetterComponent,
    CountdownComponent,
    RoundEndSummaryComponent,
    KeyboardComponent,
    StatsComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    NgxsModule.forFeature([GameState, GuessState]),
    MatProgressBarModule,
    MatProgressSpinnerModule,
  ],
})
export class GameModule {}
