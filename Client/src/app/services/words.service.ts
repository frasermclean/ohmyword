import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { PartOfSpeech } from '../models/part-of-speech.enum';
import { CreateWordRequest } from '../models/requests/create-word.request';
import { GetWordsRequest } from '../models/requests/get-words.request';
import { UpdateWordRequest } from '../models/requests/update-word.request';
import { GetWordsResponse } from '../models/responses/get-words.response';
import { WordResponse } from '../models/responses/word.response';
import { Word } from '../models/word.model';

@Injectable({
  providedIn: 'root',
})
export class WordsService {
  private readonly baseUrl = `${environment.api.baseUrl}/words`;
  constructor(private httpClient: HttpClient) {}

  public getWords(request: GetWordsRequest) {
    return this.httpClient
      .get<GetWordsResponse>(this.baseUrl, {
        params: new HttpParams()
          .set('offset', request.offset)
          .set('limit', request.limit)
          .set('filter', request.filter)
          .set('orderBy', request.orderBy)
          .set('desc', request.desc),
      })
      .pipe(
        map((response) => ({
          ...response,
          words: response.words.map((word) => new Word(word)),
        }))
      );
  }

  public createWord(request: CreateWordRequest) {
    return this.httpClient.post<WordResponse>(this.baseUrl, request).pipe(map((response) => new Word(response)));
  }

  public updateWord(request: UpdateWordRequest) {
    return this.httpClient.put<WordResponse>(this.baseUrl, request).pipe(map((response) => new Word(response)));
  }

  public deleteWord(partOfSpeech: PartOfSpeech, id: string) {
    return this.httpClient.delete<void>(`${this.baseUrl}/${partOfSpeech}/${id}`);
  }
}
