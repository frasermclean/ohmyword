import { LetterHintResponse } from './letter-hint.response';
import { PartOfSpeech } from "@models/enums";

export interface WordHintResponse {
  length: number;
  definition: string;
  partOfSpeech: PartOfSpeech;
  letterHints: LetterHintResponse[];
}
