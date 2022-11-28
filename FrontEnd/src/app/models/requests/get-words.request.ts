import { GetWordsOrderBy } from '../enums/get-words-order-by.enum';
import { SortDirection } from '../enums/sort-direction.enum';

export interface GetWordsRequest {
  offset: number;
  limit: number;
  filter: string;
  orderBy: GetWordsOrderBy;
  direction: SortDirection;
}
