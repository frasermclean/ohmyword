import { v4 as uuidv4 } from 'uuid';
import { Definition } from './definition.model';
import { WordResponse } from './responses/word.response';

export class Word {
  readonly id: string;
  readonly length: number;
  readonly definitions: Definition[];
  readonly lastModifiedTime: Date;

  constructor(init?: Partial<WordResponse>) {
    this.id = init?.id || uuidv4();
    this.length = init?.length || 0;
    this.definitions = init?.definitions?.map((response) => new Definition(response)) || [new Definition()];
    this.lastModifiedTime = init?.lastModifiedTime ? new Date(init.lastModifiedTime) : new Date();
  }
}
