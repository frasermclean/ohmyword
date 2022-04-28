import { Component, Input, OnChanges } from '@angular/core';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnChanges {
  @Input() expiryDate: Date = new Date();
  
  private expiryTime: number = 0;
  private changeTime: number = 0;
  private interval: any;
  public progressPercentage: number = 0;

  constructor() {}

  ngOnChanges(): void {
    // reset on new change
    this.expiryTime = this.expiryDate.getTime();
    this.changeTime = new Date().getTime();
    this.progressPercentage = 0;

    // clear any previous inteval
    if (this.interval) clearInterval(this.interval);
    this.interval = setInterval(() => {
      const timespan = this.expiryTime - this.changeTime;
      const elapsed = new Date().getTime() - this.changeTime;
      this.progressPercentage = Math.round((elapsed / timespan) * 100);
    }, 100);
  }
}
