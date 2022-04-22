import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-round-end-summary',
  templateUrl: './round-end-summary.component.html',
  styleUrls: ['./round-end-summary.component.scss'],
})
export class RoundEndSummaryComponent implements OnInit {
  roundEnd$ = this.gameService.roundEnd$;
  constructor(private gameService: GameService) {}

  ngOnInit(): void {}
}
