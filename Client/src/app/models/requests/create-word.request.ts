import { PartOfSpeech } from "../enums/part-of-speech.enum";

export interface CreateWordRequest {
  value: string;
  partOfSpeech: PartOfSpeech;
  definition: string;
}
