import { LetterHint } from './letter-hint';
import { WordHintResponse } from './responses/word-hint.response';

export class WordHint {
  length: number;
  definition: string;
  letters: LetterHint[] = [];

  constructor(init?: Partial<WordHintResponse>) {
    this.length = init?.length ?? 0;
    this.definition = init?.definition ?? '';
  }

  public static default = new WordHint();
}
