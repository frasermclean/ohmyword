import { PartOfSpeech } from "../enums/part-of-speech.enum";

export interface UpdateWordRequest {
  id: string;
  value: string;
  partOfSpeech: PartOfSpeech;
  definition: string;
}