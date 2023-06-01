import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { RouterModule, Routes } from '@angular/router';
import { NgxsModule } from '@ngxs/store';

// material modules
import { MatButtonModule } from '@angular/material/button';
import { MatInputModule } from '@angular/material/input';
import { MatProgressBarModule } from '@angular/material/progress-bar';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatTableModule } from "@angular/material/table";
import { MatTooltipModule } from "@angular/material/tooltip";

// states
import { GameState } from '@state/game/game.state';
import { GuessState } from '@state/guess/guess.state';
import { HubState } from '@state/hub/hub.state';

// components
import { GameRootComponent } from './game.component';
import { HintComponent } from './hint/hint.component';
import { GuessComponent } from './guess/guess.component';
import { LetterComponent } from './hint/letter/letter.component';
import { CountdownComponent } from './countdown/countdown.component';
import { RoundSummaryComponent } from './round-summary/round-summary.component';
import { StatsComponent } from './stats/stats.component';
import { SpinnerComponent } from './spinner/spinner.component';
import { RoundEndReasonComponent } from './round-summary/round-end-reason/round-end-reason.component';
import { WordRevealComponent } from './round-summary/word-reveal/word-reveal.component';
import { RoundScoresComponent } from './round-summary/round-scores/round-scores.component';

const routes: Routes = [{path: '', component: GameRootComponent}];

@NgModule({
  declarations: [
    HintComponent,
    GuessComponent,
    GameRootComponent,
    LetterComponent,
    CountdownComponent,
    RoundSummaryComponent,
    StatsComponent,
    SpinnerComponent,
    RoundEndReasonComponent,
    WordRevealComponent,
    RoundScoresComponent,
  ],
  imports: [
    CommonModule,
    RouterModule.forChild(routes),
    ReactiveFormsModule,
    NgxsModule.forFeature([GameState, GuessState, HubState]),
    MatButtonModule,
    MatInputModule,
    MatProgressBarModule,
    MatProgressSpinnerModule,
    MatTableModule,
    MatTooltipModule,
  ],
})
export class GameModule {
}
