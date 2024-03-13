import { Component, Inject, OnInit } from '@angular/core';
import { NonNullableFormBuilder, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { take, tap } from 'rxjs/operators';
import { Definition } from '@models/definition.model';
import { Word } from '@models/word.model';
import { PartOfSpeech } from "@models/enums";
import { WordsService } from '@services/words.service';

const partOfSpeechOptions = [
  { value: PartOfSpeech.Noun, label: 'Noun' },
  { value: PartOfSpeech.Verb, label: 'Verb' },
  { value: PartOfSpeech.Adjective, label: 'Adjective' },
  { value: PartOfSpeech.Adverb, label: 'Adverb' },
];

export interface WordEditData {
  word: Word;
}

export interface WordEditResult {
  id: string;
  frequency: number;
  definitions: Definition[];
}

@Component({
  selector: 'app-word-edit',
  templateUrl: './word-edit.component.html',
  styleUrls: ['./word-edit.component.scss'],
})
export class WordEditComponent implements OnInit {
  readonly word: Word = this.data?.word || new Word();
  readonly isEditing = !!this.data?.word;
  readonly partOfSpeechOptions = partOfSpeechOptions;

  formGroup = this.formBuilder.group({
    id: [this.word.id, [Validators.required]],
    frequency: [this.word.frequency, [Validators.required, Validators.min(1), Validators.max(7)]],
    definitions: this.formBuilder.array(
      this.word.definitions.map((definition) =>
        this.formBuilder.group({
          id: [definition.id],
          partOfSpeech: [definition.partOfSpeech, Validators.required],
          value: [definition.value, Validators.required],
          example: [definition.example],
        })
      )
    ),
  });

  get definitions() {
    return this.formGroup.controls.definitions;
  }

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: WordEditData,
    private formBuilder: NonNullableFormBuilder,
    private dialogRef: MatDialogRef<WordEditComponent, WordEditResult>,
    private wordsService: WordsService
  ) { }

  ngOnInit(): void {
    if (this.isEditing) this.formGroup.controls.id.disable(); // can't edit id
  }

  getDefinitionSuggestions(wordId: string) {
    this.wordsService
      .getWord(wordId, true)
      .pipe(
        tap((word) => {
            this.formGroup.controls.frequency.setValue(word.frequency);
            word.definitions.forEach((definition) => this.addDefinition(definition));
          }
        ),
        take(1)
      )
      .subscribe();
  }

  addDefinition(definition?: Definition) {
    this.definitions.push(
      this.formBuilder.group({
        id: [definition?.id || ''],
        partOfSpeech: [definition?.partOfSpeech || PartOfSpeech.Noun, Validators.required],
        value: [definition?.value || '', Validators.required],
        example: [definition?.example || ''],
      })
    );
  }

  removeDefinition(index: number) {
    this.definitions.removeAt(index);
  }

  submit() {
    this.dialogRef.close({
      id: this.formGroup.controls.id.value,
      frequency: this.formGroup.controls.frequency.value,
      definitions: this.formGroup.controls.definitions.value.map((d) => new Definition(d)),
    });
  }
}
