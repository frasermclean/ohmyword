import { Component, Inject, OnInit } from '@angular/core';
import { NonNullableFormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Definition } from 'src/app/models/definition.model';
import { PartOfSpeech } from 'src/app/models/enums/part-of-speech.enum';
import { Word } from 'src/app/models/word.model';

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
    id: [this.word.id, Validators.required],
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
    private dialogRef: MatDialogRef<WordEditComponent, WordEditResult>
  ) {}

  ngOnInit(): void {
    if (this.isEditing) this.formGroup.controls.id.disable(); // can't edit id
  }

  addDefinition() {
    this.definitions.push(
      this.formBuilder.group({
        id: [''],
        partOfSpeech: [PartOfSpeech.Noun, Validators.required],
        value: ['', Validators.required],
        example: [''],
      })
    );
  }

  removeDefinition(index: number) {
    this.definitions.removeAt(index);
  }

  submit() {
    this.dialogRef.close({
      id: this.formGroup.controls.id.value,
      definitions: this.formGroup.controls.definitions.value.map((d) => new Definition(d)),
    });
  }
}
