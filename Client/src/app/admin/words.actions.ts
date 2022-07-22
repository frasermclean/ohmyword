import { PartOfSpeech } from '../models/part-of-speech.enum';
import { CreateWordRequest } from '../models/requests/create-word.request';
import { GetWordsRequest } from '../models/requests/get-words.request';
import { UpdateWordRequest } from '../models/requests/update-word.request';

export namespace Words {
  export class GetWords {
    static readonly type = '[Words List] Get Words';
    constructor(public request: Partial<GetWordsRequest>) {}
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

  export class Search {
    static readonly type = '[Words List] Search';
    constructor(public filter: string) {}
  }
}
