export class LetterHint {
  readonly position: number;
  readonly value: string;

  constructor(position: number, value: string) {
    this.position = position;
    this.value = value;
  }
}
