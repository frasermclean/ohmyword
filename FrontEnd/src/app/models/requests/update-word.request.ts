import { Definition } from "../definition.model";

export interface UpdateWordRequest {
  id: string;
  definitions: Definition[];
}
