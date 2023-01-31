import { Component, Input, OnDestroy, OnInit } from '@angular/core';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnInit, OnDestroy {
  @Input() startDate: Date = new Date();
  @Input() endDate: Date = new Date();
  @Input() updateRate = 100;

  private intervalId: any;
  public progressPercentage: number = 0;

  ngOnInit() {
    const startTime = this.startDate.getTime();
    const endTime = this.endDate.getTime();
    const timespan = endTime - startTime;

    this.intervalId = setInterval(() => {
      const now = new Date().getTime();
      const elapsed = now - startTime;
      this.progressPercentage = Math.round((elapsed / timespan) * 100);
    }, this.updateRate);
  }

  ngOnDestroy() {
    clearInterval(this.intervalId);
  }
}
