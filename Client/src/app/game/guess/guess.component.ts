import {
  Component,
  ElementRef,
  EventEmitter,
  Input,
  OnDestroy,
  OnInit,
  Output,
  ViewChild,
} from '@angular/core';
import { FormControl } from '@angular/forms';
import { Subscription } from 'rxjs';
import { GuessResponse } from 'src/app/models/responses/guess.response';
import { WordHint } from 'src/app/models/word-hint';

import { GameService } from 'src/app/services/game.service';

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit, OnDestroy {
  inputControl = new FormControl('');
  response: GuessResponse | null = null;

  subscription: Subscription = null!;

  @Input()
  hint: WordHint = null!;

  @Output()
  valueChanged = new EventEmitter<string>();

  @ViewChild('input')
  inputElement: ElementRef<HTMLInputElement> = null!;

  constructor(private gameService: GameService) {}

  ngOnInit(): void {
    if (!this.hint) throw new Error('Hint has not been set!');
    this.subscription = this.inputControl.valueChanges.subscribe((value) => {
      if (typeof value !== 'string') return;
      this.valueChanged.emit(value);
    });
  }

  ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  onInputBlur(event: FocusEvent) {
    if (this.inputElement) {
      this.inputElement.nativeElement.focus();
    }
  }

  async onEnterKeyDown() {
    const value =
      typeof this.inputControl.value === 'string'
        ? this.inputControl.value.trim()
        : '';

    if (value.length === this.hint.length) {
      this.response = await this.gameService.guessWord(value);
      this.inputControl.reset('');
      setTimeout(() => {
        this.response = null;
      }, 2000);
    }
  }
}
