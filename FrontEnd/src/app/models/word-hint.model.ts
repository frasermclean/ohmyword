import { WordHintResponse } from './responses/word-hint.response';
import { PartOfSpeech } from "@models/enums";

export class WordHint {
  readonly length: number;
  readonly definition: string;
  readonly partOfSpeech: PartOfSpeech;
  readonly letterHints: string[];

  constructor(response: WordHintResponse) {
    this.length = response.length ?? 0;
    this.definition = response.definition ?? '';
    this.partOfSpeech = response.partOfSpeech ?? PartOfSpeech.Noun;
    this.letterHints = new Array(this.length).fill('');

    response.letterHints.forEach((lh) => {
      this.letterHints[lh.position - 1] = lh.value;
    });
  }

  public static default = new WordHint({
    length: 7,
    definition: 'Default word',
    partOfSpeech: PartOfSpeech.Noun,
    letterHints: [],
  });
}
