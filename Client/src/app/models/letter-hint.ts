import { LetterHintResponse } from './responses/letter-hint.response';

export class LetterHint {
  position: number;
  value: string;

  constructor(response: LetterHintResponse) {
    this.position = response.position;
    this.value = response.value;
  }
}
