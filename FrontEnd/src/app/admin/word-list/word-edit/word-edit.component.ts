import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormControl, NonNullableFormBuilder, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { Definition } from 'src/app/models/definition.model';
import { Word } from 'src/app/models/word.model';

@Component({
  selector: 'app-word-edit',
  templateUrl: './word-edit.component.html',
  styleUrls: ['./word-edit.component.scss'],
})
export class WordEditComponent implements OnInit {
  readonly word: Word = this.data?.word || new Word();
  readonly isEditing = !!this.data?.word;

  idControl = new FormControl(this.word.id, Validators.required);

  constructor(
    @Inject(MAT_DIALOG_DATA) private data: { word: Word },

    private dialogRef: MatDialogRef<WordEditComponent>
  ) {}

  ngOnInit(): void {
    if (this.isEditing) this.idControl.disable(); // can't edit id
  }

  addDefinition() {
    this.word.definitions.push(new Definition());
  }

  removeDefinition(index: number) {
    this.word.definitions.splice(index, 1);
  }

  onDefinitionChanged(definition: Definition, index: number) {
    this.word.definitions[index] = definition;
  }

  onDefinitionValidChanged(isValid: boolean, index: number) {
    // TODO: Implement definition validity check
  }

  submit() {
    this.dialogRef.close({
      id: this.idControl.value,
      definitions: this.word.definitions,
    });
  }
}
