import { PartOfSpeech } from "@models/enums";

export interface DefinitionResponse {
  id: string;
  partOfSpeech: PartOfSpeech;
  value: string;
  example: string;
}
