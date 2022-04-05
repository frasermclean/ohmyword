import { Component, Input, OnChanges, OnInit, SimpleChanges } from '@angular/core';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnChanges {
  @Input() expiryDate: Date = null!;
  secondsRemaining: number = 0;
  interval: any;

  constructor() {}

  ngOnChanges(): void {
    if (!this.expiryDate) throw new Error('Expiry date must be provided!');
    
    const expiryTime = this.expiryDate.getTime();
    this.secondsRemaining = this.getSecondsRemaining(expiryTime);

    // clear any previous inteval
    if (this.interval) clearInterval(this.interval);

    this.interval = setInterval(() => {
      this.secondsRemaining = this.getSecondsRemaining(expiryTime);
    }, 1000);
  }

  private getSecondsRemaining(expiry: number) {
    const now = new Date().getTime();
    const difference = expiry - now;
    return Math.round((difference % (1000 * 60)) / 1000);
  }
}
