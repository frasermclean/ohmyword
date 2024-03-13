import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormControl, NonNullableFormBuilder } from '@angular/forms';
import { Store } from '@ngxs/store';
import { Guess } from '@state/guess/guess.actions';
import { GuessState } from "@state/guess/guess.state";
import { takeUntil, tap } from "rxjs/operators";
import { Subject } from "rxjs";

@Component({
  selector: 'app-guess',
  templateUrl: './guess.component.html',
  styleUrls: ['./guess.component.scss'],
})
export class GuessComponent implements OnInit, OnDestroy {
  formGroup = this.formBuilder.group({
    guess: ''
  })

  maxLength$ = this.store.select(GuessState.maxLength);
  private destroying$ = new Subject<void>();

  constructor(private store: Store, private formBuilder: NonNullableFormBuilder) {
  }

  ngOnInit(): void {
    this.formGroup.controls.guess.valueChanges.pipe(
      takeUntil(this.destroying$),
      tap(value => this.store.dispatch(new Guess.SetValue(value)))
    ).subscribe();
  }

  ngOnDestroy(): void {
    this.destroying$.next();
    this.destroying$.complete();
  }

  submitGuess() {
    const value = this.formGroup.controls.guess.value;
    this.store.dispatch(new Guess.Submit(value));
    this.formGroup.reset();
  }
}
