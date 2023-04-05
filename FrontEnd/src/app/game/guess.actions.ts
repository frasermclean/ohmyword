import { WordHint } from "../models/word-hint.model";

/**
 * Guess actions and events
 */
export namespace Guess {
  export class Append {
    static readonly type = '[Guess Component] Guess.Append';
    constructor(public value: string) {}
  }

  export class Backspace {
    static readonly type = '[Guess Component] Guess.Backspace';
  }

  export class Submit {
    static readonly type = '[Guess Component] Guess.Submit';
  }

  export class Succeeded {
    static readonly type = '[Game Service] Guess.Succeeded';
    constructor(public points: number) {}
  }

  export class Failed {
    static readonly type = '[Game Service] Guess.Failed';
  }

  export class SetNewWord {
    static readonly type = '[Game Service] Guess.SetNewWord';
    constructor(public wordHint: WordHint) {}
  }
}
