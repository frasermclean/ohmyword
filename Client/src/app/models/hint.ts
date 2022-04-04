import { HintResponse } from './responses/hint.response';

export class Hint {
  length: number;
  definition: string;
  expiry: Date;

  constructor(response: HintResponse) {
    this.length = response.length;
    this.definition = response.definition;
    this.expiry = new Date(response.expiry);
  }
}
