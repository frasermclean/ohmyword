import { Component, Input } from '@angular/core';
import { ScoreLine } from "@models/round-summary.model";

@Component({
  selector: 'app-round-scores',
  templateUrl: './round-scores.component.html',
  styleUrls: ['./round-scores.component.scss']
})
export class RoundScoresComponent {
  @Input() scores: ScoreLine[] = [];

  readonly displayedColumns: string[] = ['playerName', 'guessCount', 'pointsAwarded'];
}
