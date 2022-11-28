import { GetWordsOrderBy } from '../enums/get-words-order-by.enum';
import { SortDirection } from '../enums/sort-direction.enum';
import { WordResponse } from './word.response';

export interface GetWordsResponse {
  offset: number;
  limit: number;
  total: number;
  filter: string;
  orderBy: GetWordsOrderBy;
  direction: SortDirection;
  words: WordResponse[];
}
