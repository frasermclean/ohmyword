import { PartOfSpeech } from '../enums/part-of-speech.enum';

export interface DefinitionResponse {
  id: string;
  partOfSpeech: PartOfSpeech;
  value: string;
  example: string;
}
