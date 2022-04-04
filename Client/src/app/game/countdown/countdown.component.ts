import { Component, Input, OnInit } from '@angular/core';
import { HintResponse } from 'src/app/models/hint.response';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnInit {
  @Input() hint: HintResponse = null!;
  secondsRemaining: number = 0;
  interval: any;

  constructor() {}

  ngOnInit(): void {
    if (!this.hint) throw new Error('Hint must be provided');

    const expiry = new Date(this.hint.expiry).getTime();
    this.secondsRemaining = this.getSecondRemaining(expiry);

    // clear any previous inteval
    if (this.interval) clearInterval(this.interval);

    this.interval = setInterval(() => {
      this.secondsRemaining = this.getSecondRemaining(expiry);
    }, 1000);
  }

  private getSecondRemaining(expiry: number) {
    const now = new Date().getTime();
    const difference = expiry - now;
    return Math.floor((difference % (1000 * 60)) / 1000);
  }
}
