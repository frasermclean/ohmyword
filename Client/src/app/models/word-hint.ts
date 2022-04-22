import { LetterHint } from './letter-hint';
import { WordHintResponse } from './responses/word-hint.response';

export class WordHint {
  readonly length: number;
  readonly definition: string;
  readonly letters: LetterHint[] = [];

  constructor(init?: Partial<WordHintResponse>) {
    this.length = init?.length ?? 0;
    this.definition = init?.definition ?? '';
    if (init?.letters) {
      for (const letter of init.letters) {
        this.letters.push(new LetterHint(letter));
      }
    }
  }

  public static default = new WordHint();

  public addLetterHint(letterHint: LetterHint) {
    this.letters.push(letterHint);
    this.letters.sort((a, b) => a.position - b.position);
  }
}
