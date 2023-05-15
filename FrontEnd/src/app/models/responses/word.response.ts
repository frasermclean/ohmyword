import { DefinitionResponse } from "./definition-response";
import { PartOfSpeech } from "@models/enums";

export interface WordResponse {
  id: string;
  length: number;
  definitions: DefinitionResponse[];
  lastModifiedTime: string;
}
