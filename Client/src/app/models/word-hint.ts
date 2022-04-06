import { LetterHint } from './letter-hint';
import { WordHintResponse } from './responses/word-hint.response';

export class WordHint {
  readonly length: number;
  readonly definition: string;
  readonly letters: LetterHint[] = [];

  constructor(init?: Partial<WordHintResponse>) {
    this.length = init?.length ?? 0;
    this.definition = init?.definition ?? '';
  }

  public static default = new WordHint();
}
