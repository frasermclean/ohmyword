import { WordHint } from '@models/word-hint.model';

/**
 * Guess actions and events
 */
export namespace Guess {

  export class SetValue {
    static readonly type = '[Guess Component] Guess.SetValue';

    constructor(public value: string) {
    }
  }

  export class Submit {
    static readonly type = '[Guess Component] Guess.Submit';

    constructor(public value: string) {
    }
  }

  export class Succeeded {
    static readonly type = '[Game Service] Guess.Succeeded';

    constructor(public points: number) {
    }
  }

  export class Failed {
    static readonly type = '[Game Service] Guess.Failed';

    constructor(public message: string) {
    }
  }
}
