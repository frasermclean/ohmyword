import { PartOfSpeech } from "../enums/part-of-speech.enum";
import { DefinitionResponse } from "./definition-response";

export interface WordResponse {
  id: string;
  length: number;
  definitions: DefinitionResponse[];
  lastModifiedTime: string;
}
