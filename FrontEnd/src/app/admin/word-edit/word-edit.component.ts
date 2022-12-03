import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { PartOfSpeech } from 'src/app/models/enums/part-of-speech.enum';
import { Word } from 'src/app/models/word.model';

const partOfSpeechOptions = [
  { value: PartOfSpeech.Noun, label: 'Noun' },
  { value: PartOfSpeech.Verb, label: 'Verb' },
  { value: PartOfSpeech.Adjective, label: 'Adjective' },
  { value: PartOfSpeech.Adverb, label: 'Adverb' },
];

@Component({
  selector: 'app-word-edit',
  templateUrl: './word-edit.component.html',
  styleUrls: ['./word-edit.component.scss'],
})
export class WordEditComponent implements OnInit {
  readonly isEditing: boolean;
  readonly formGroup = this.formBuilder.group({
    value: [this.data?.word.value, Validators.required],
    partOfSpeech: [this.data?.word.partOfSpeech, Validators.required],
    definition: [this.data?.word.definition, Validators.required],
  });
  readonly partOfSpeechOptions = partOfSpeechOptions;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: { word: Word },
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<WordEditComponent>
  ) {
    this.isEditing = !!data?.word;
  }

  ngOnInit(): void {}

  submit() {
    this.dialogRef.close(this.formGroup.value);
  }
}
