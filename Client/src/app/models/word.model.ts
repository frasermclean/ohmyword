import { PartOfSpeech } from './part-of-speech.enum';
import { WordResponse } from './responses/word.response';

export class Word {
  readonly id: string;
  readonly value: string;
  readonly partOfSpeech: PartOfSpeech;
  readonly definition: string;
  readonly lastModifiedTime: Date;

  constructor(response: WordResponse) {
    this.id = response.id;
    this.value = response.value;
    this.partOfSpeech = response.partOfSpeech;
    this.definition = response.definition;
    this.lastModifiedTime = new Date(response.lastModifiedTime);
  }
}
