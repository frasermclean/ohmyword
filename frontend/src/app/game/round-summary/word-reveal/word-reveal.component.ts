import { Component, Input } from '@angular/core';
import { PartOfSpeech } from "@models/enums";

@Component({
  selector: 'app-word-reveal',
  templateUrl: './word-reveal.component.html',
  styleUrls: ['./word-reveal.component.scss']
})
export class WordRevealComponent {
  @Input() word: string = '';
  @Input() partOfSpeech: PartOfSpeech = PartOfSpeech.Noun;
}
