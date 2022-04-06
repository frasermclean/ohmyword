import { Component, Input, OnInit } from '@angular/core';
import { LetterData } from '../hint.component';

@Component({
  selector: 'app-letter',
  templateUrl: './letter.component.html',
  styleUrls: ['./letter.component.scss'],
})
export class LetterComponent implements OnInit {
  @Input() data: LetterData = null!;

  constructor() {}

  ngOnInit(): void {
    if (!this.data) throw new Error('Letter data input is not set!');
  }

  getState(): 'correct' | 'incorrect' | 'hint' | 'default' {
    if (this.data.hintValue) {
      if (this.data.guessValue) {
        return this.data.guessValue === this.data.hintValue
          ? 'correct'
          : 'incorrect';
      }
      return 'hint';
    }

    return 'default';
  }
}
