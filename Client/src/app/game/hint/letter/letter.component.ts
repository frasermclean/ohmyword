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
}
