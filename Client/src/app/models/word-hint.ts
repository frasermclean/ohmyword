import { LetterHint } from './letter-hint';
import { WordHintResponse } from './responses/word-hint.response';

export class WordHint {
  readonly length: number;
  readonly definition: string;
  readonly letterHints: string[];

  constructor(init?: Partial<WordHintResponse>) {
    this.length = init?.length ?? 0;
    this.definition = init?.definition ?? '';
    this.letterHints = new Array(this.length).fill('');

    // if (init?.letters) {
    //   for (const letter of init.letters) {
    //     this.letterHints.push(new LetterHint(letter.position, letter.value));
    //   }
    // }
  }

  public static default = new WordHint();
}
