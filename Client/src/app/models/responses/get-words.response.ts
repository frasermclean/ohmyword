import { WordResponse } from './word.response';

export interface GetWordsResponse {
  offset: number;
  limit: number;
  total: number;
  words: WordResponse[];
}
