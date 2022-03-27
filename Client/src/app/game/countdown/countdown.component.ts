import { Component, OnInit } from '@angular/core';
import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnInit {
  secondsRemaining: number = 0;
  interval: any;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    this.gameService.hint$.subscribe((hint) => {
      const expiry = new Date(hint.expiry).getTime();
      this.secondsRemaining = this.getSecondRemaining(expiry);
      
      // clear any previous inteval
      if (this.interval) clearInterval(this.interval);

      this.interval = setInterval(() => {
        this.secondsRemaining = this.getSecondRemaining(expiry);
      }, 1000);
    });
  }

  private getSecondRemaining(expiry: number) {
    const now = new Date().getTime();
    const difference = expiry - now;
    return Math.floor((difference % (1000 * 60)) / 1000);
  }
}
