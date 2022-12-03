import { PartOfSpeech } from "../enums/part-of-speech.enum";

export interface WordResponse {
  id: string;
  value: string;
  partOfSpeech: PartOfSpeech;
  definition: string;
  lastModifiedTime: string;
}
