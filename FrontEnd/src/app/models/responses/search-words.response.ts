import { WordResponse } from './word.response';

export interface SearchWordsResponse {
  total: number;
  words: WordResponse[];
}
