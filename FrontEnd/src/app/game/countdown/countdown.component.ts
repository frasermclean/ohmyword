import { Component, Input, OnChanges, OnDestroy } from '@angular/core';

@Component({
  selector: 'app-countdown',
  templateUrl: './countdown.component.html',
  styleUrls: ['./countdown.component.scss'],
})
export class CountdownComponent implements OnChanges, OnDestroy {
  @Input() startDate: Date = new Date();
  @Input() endDate: Date = new Date();

  private intervalId: any;
  public progressPercentage: number = 0;
  public secondsRemaining: number = 0;

  ngOnChanges() {
    if (this.intervalId) clearInterval(this.intervalId);

    const startTime = this.startDate.getTime();
    const endTime = this.endDate.getTime();
    const timespan = endTime - startTime;

    this.intervalId = setInterval(() => {
      const now = new Date().getTime();
      const elapsed = now - startTime;
      this.progressPercentage = Math.round((elapsed / timespan) * 100);
      this.secondsRemaining = Math.round((endTime - now) / 1000);
    }, 100);
  }

  ngOnDestroy() {
    clearInterval(this.intervalId);
  }
}
