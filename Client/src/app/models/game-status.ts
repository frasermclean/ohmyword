import { Hint } from './hint';

export interface GameStatus {
  roundStatus: 'active' | 'complete';
  expiry: Date;
  hint: Hint | null;
}
