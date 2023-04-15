import { PartOfSpeech } from '@models/enums/part-of-speech.enum';
import { RoundEndReason } from '@models/enums/round-end-reason';

export interface RoundSummaryResponse {
  word: string;
  partOfSpeech: PartOfSpeech;
  endReason: RoundEndReason;
}
