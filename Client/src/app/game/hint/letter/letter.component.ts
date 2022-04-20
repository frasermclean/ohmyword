import { Component, Input, OnInit } from '@angular/core';
import { LetterData } from '../hint.component';

@Component({
  selector: 'app-letter',
  templateUrl: './letter.component.html',
  styleUrls: ['./letter.component.scss'],
})
export class LetterComponent implements OnInit {
  @Input() data: LetterData = null!;
  @Input() position: number = 0;

  constructor() {}

  ngOnInit(): void {
    if (!this.data) throw new Error('Letter data input is not set!');
  }

  getState(): 'correct' | 'incorrect' | 'hint' | 'default' {
    if (this.data.hint) {
      if (this.data.guess) {
        return this.data.guess === this.data.hint
          ? 'correct'
          : 'incorrect';
      }
      return 'hint';
    }

    return 'default';
  }
}
