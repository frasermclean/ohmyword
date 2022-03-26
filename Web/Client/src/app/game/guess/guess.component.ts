import { Component, OnInit } from '@angular/core';
import { FormControl } from '@angular/forms';

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit {
  guess = new FormControl('');

  constructor() {}

  ngOnInit(): void {
  }

  onGuess(value: string) {
    console.log(value);
    this.guess.reset();
  }
}
