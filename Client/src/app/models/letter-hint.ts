import { LetterHintResponse } from './responses/letter-hint.response';

export class LetterHint {
  position: number;
  value: string;

  constructor(init?: Partial<LetterHintResponse>) {
    this.position = init?.position ?? 0;
    this.value = init?.value ?? '';
  }
}
