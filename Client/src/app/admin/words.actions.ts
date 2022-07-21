import { PartOfSpeech } from '../models/part-of-speech.enum';
import { CreateWordRequest } from '../models/requests/create-word.request';
import { UpdateWordRequest } from '../models/requests/update-word.request';

export namespace Words {
  export class GetWords {
    static readonly type = '[Words List] Get Words';
    constructor(public offset?: number, public limit?: number) {}
  }

  export class CreateWord {
    static readonly type = '[Words List] Create Word';
    constructor(public request: CreateWordRequest) {}
  }

  export class UpdateWord {
    static readonly type = '[Words List] Update Word';
    constructor(public request: UpdateWordRequest) {}
  }

  export class DeleteWord {
    static readonly type = '[Words List] Delete Word';
    constructor(public partOfSpeech: PartOfSpeech, public id: string) {}
  }
}
