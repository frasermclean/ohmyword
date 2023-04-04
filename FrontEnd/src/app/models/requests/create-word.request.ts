import { Definition } from "../definition.model";

export interface CreateWordRequest {
  id: string;
  definitions: Definition[];
}
