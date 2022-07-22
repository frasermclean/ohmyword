import { WordResponse } from './word.response';

export interface GetWordsResponse {
  offset: number;
  limit: number;
  total: number;
  filter: string | null;
  orderBy: string | null;
  desc: boolean;
  words: WordResponse[];
}
