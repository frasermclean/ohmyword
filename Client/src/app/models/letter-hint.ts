import { LetterHintResponse } from './responses/letter-hint.response';

export class LetterHint {
  readonly position: number;
  readonly value: string;

  constructor(init?: Partial<LetterHintResponse>) {
    this.position = init?.position ?? 0;
    this.value = init?.value ?? '';
  }

  public static default = new LetterHint();
}
