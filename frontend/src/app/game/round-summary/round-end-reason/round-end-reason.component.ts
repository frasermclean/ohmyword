import { Component, Input } from '@angular/core';
import { RoundEndReason } from "@models/enums";

@Component({
  selector: 'app-round-end-reason',
  templateUrl: './round-end-reason.component.html',
  styleUrls: ['./round-end-reason.component.scss']
})
export class RoundEndReasonComponent {
  @Input() reason: RoundEndReason = RoundEndReason.Timeout;
}
