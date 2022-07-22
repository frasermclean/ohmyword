export interface GetWordsRequest {
  offset: number;
  limit: number;
  filter: string;
  orderBy: string;
  desc: boolean;
}
