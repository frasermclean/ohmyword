import { CreateWordRequest } from '../models/requests/create-word.request';
import { SearchWordsRequest } from '../models/requests/search-words.request';
import { UpdateWordRequest } from '../models/requests/update-word.request';

export namespace Words {
  export class SearchWords {
    static readonly type = '[Words List] Search Words';
    constructor(public request: Partial<SearchWordsRequest>) {}
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
    constructor(public id: string) {}
  }
}
