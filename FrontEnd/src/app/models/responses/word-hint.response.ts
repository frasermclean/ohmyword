import { PartOfSpeech } from '@models/enums/part-of-speech.enum';
import { LetterHintResponse } from './letter-hint.response';

export interface WordHintResponse {
  length: number;
  definition: string;
  partOfSpeech: PartOfSpeech;
  letterHints: LetterHintResponse[];
}
