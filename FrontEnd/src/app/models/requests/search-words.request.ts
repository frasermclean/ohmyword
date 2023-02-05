import { SearchWordsOrderBy } from '../enums/search-words-order-by.enum';
import { SortDirection } from '../enums/sort-direction.enum';

export interface SearchWordsRequest {
  offset: number;
  limit: number;
  filter: string;
  orderBy: SearchWordsOrderBy;
  direction: SortDirection;
}
