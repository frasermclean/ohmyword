import { PartOfSpeech } from './enums/part-of-speech.enum';
import { WordResponse } from './responses/word.response';

export class Word {
  readonly id: string;
  readonly value: string;
  readonly partOfSpeech: PartOfSpeech;
  readonly definition: string;
  readonly lastModifiedTime: Date;

  constructor(init?: Partial<WordResponse>) {
    this.id = init?.id || '';
    this.value = init?.value || '';
    this.partOfSpeech = init?.partOfSpeech || PartOfSpeech.Noun;
    this.definition = init?.definition || '';
    this.lastModifiedTime = init?.lastModifiedTime ? new Date(init.lastModifiedTime) : new Date();
  }
}
