import { Component, EventEmitter, Input, OnDestroy, OnInit, Output } from '@angular/core';
import { FormBuilder, NonNullableFormBuilder, Validators } from '@angular/forms';
import { Subject } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Definition } from '@models/definition.model';
import { PartOfSpeech } from '@models/enums/part-of-speech.enum';

const partOfSpeechOptions = [
  { value: PartOfSpeech.Noun, label: 'Noun' },
  { value: PartOfSpeech.Verb, label: 'Verb' },
  { value: PartOfSpeech.Adjective, label: 'Adjective' },
  { value: PartOfSpeech.Adverb, label: 'Adverb' },
];

@Component({
  selector: 'app-definition-edit',
  templateUrl: './definition-edit.component.html',
  styleUrls: ['./definition-edit.component.scss'],
})
export class DefinitionEditComponent implements OnInit, OnDestroy {
  @Input() definition = new Definition();
  @Output() definitionChange = new EventEmitter<Definition>();
  @Output() validChange = new EventEmitter<boolean>();

  readonly partOfSpeechOptions = partOfSpeechOptions;

  private readonly destroy$ = new Subject<void>();

  formGroup = this.formBuilder.group({
    partOfSpeech: [this.definition.partOfSpeech, Validators.required],
    value: [this.definition.value, Validators.required],
    example: [this.definition.example],
  });
  constructor(private formBuilder: NonNullableFormBuilder) {}

  ngOnInit(): void {
    this.formGroup.setValue({
      partOfSpeech: this.definition.partOfSpeech,
      value: this.definition.value,
      example: this.definition.example,
    });

    this.formGroup.valueChanges.pipe(takeUntil(this.destroy$)).subscribe((value) => {
      this.definitionChange.emit({ ...this.definition, ...value });
    });

    this.formGroup.statusChanges.pipe(takeUntil(this.destroy$)).subscribe((status) => {
      this.validChange.emit(status === 'VALID');
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
