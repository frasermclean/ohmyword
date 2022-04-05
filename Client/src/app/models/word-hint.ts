import { LetterHint } from './letter-hint';
import { WordHintResponse } from './responses/word-hint.response';

export class WordHint {
  length: number;
  definition: string;
  letters: LetterHint[] = [];

  constructor(response: WordHintResponse) {
    this.length = response.length;
    this.definition = response.definition;
  }

  public static default = new WordHint({
    length: 0,
    definition: '',
  });
}
