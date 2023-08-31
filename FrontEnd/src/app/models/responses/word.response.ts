import { DefinitionResponse } from "./definition-response";

export interface WordResponse {
  id: string;
  length: number;
  frequency: number;
  definitions: DefinitionResponse[];
  lastModifiedTime: string;
}
