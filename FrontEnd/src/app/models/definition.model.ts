import { v4 as uuidv4 } from 'uuid';
import { PartOfSpeech } from './enums/part-of-speech.enum';
import { DefinitionResponse } from './responses/definition-response';

export class Definition {
  readonly id: string;
  readonly partOfSpeech: PartOfSpeech;
  readonly value: string;
  readonly example: string;

  constructor(init?: Partial<DefinitionResponse>) {
    this.id = init?.id || uuidv4();
    this.partOfSpeech = init?.partOfSpeech || PartOfSpeech.Noun;
    this.value = init?.value || '';
    this.example = init?.example || '';
  }
}
