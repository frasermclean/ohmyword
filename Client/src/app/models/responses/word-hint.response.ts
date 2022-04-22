import { LetterHintResponse } from './letter-hint.response';

export interface WordHintResponse {
  length: number;
  definition: string;
  letters: LetterHintResponse[];
}
