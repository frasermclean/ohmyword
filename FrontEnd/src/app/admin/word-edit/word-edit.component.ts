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
    id: [this.data?.wordId, Validators.required],
  });
  readonly partOfSpeechOptions = partOfSpeechOptions;

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: { wordId: string },
    private formBuilder: FormBuilder,
    private dialogRef: MatDialogRef<WordEditComponent>
  ) {
    this.isEditing = !!data?.wordId;
  }

  ngOnInit(): void {
    if (this.isEditing) this.formGroup.get('id')?.disable(); // can't edit id
  }

  submit() {
    this.dialogRef.close(this.formGroup.value);
  }
}
