import {Component, Inject, OnInit} from '@angular/core';
import {MAT_DIALOG_DATA} from "@angular/material/dialog";

@Component({
  selector: 'app-confirmation-prompt',
  templateUrl: './confirmation-prompt.component.html',
  styleUrls: ['./confirmation-prompt.component.scss']
})
export class ConfirmationPromptComponent implements OnInit {
  title: string;
  question: string;

  constructor(@Inject(MAT_DIALOG_DATA) data?: ConfirmationPromptData) {
    this.title = data?.title || 'Please confirm';
    this.question = data?.question || 'Do you wish to proceed?'
  }

  ngOnInit(): void {
  }
}

export interface ConfirmationPromptData {
  title: string;
  question: string;
}
