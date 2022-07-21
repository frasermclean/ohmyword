import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
import { PartOfSpeech } from '../models/part-of-speech.enum';
import { CreateWordRequest } from '../models/requests/create-word.request';
import { UpdateWordRequest } from '../models/requests/update-word.request';
import { WordResponse } from '../models/responses/word.response';
import { Word } from '../models/word.model';

@Injectable({
  providedIn: 'root',
})
export class WordsService {
  private readonly baseUrl = `${environment.api.baseUrl}/words`;
  constructor(private httpClient: HttpClient) {}

  public getAllWords() {
    return this.httpClient
      .get<WordResponse[]>(this.baseUrl)
      .pipe(map((responseArray) => responseArray.map((response) => new Word(response))));
  }

  public createWord(request: CreateWordRequest) {
    return this.httpClient
      .post<WordResponse>(this.baseUrl, request)
      .pipe(map((response) => new Word(response)));
  }

  public updateWord(request: UpdateWordRequest) {
    return this.httpClient
      .put<WordResponse>(this.baseUrl, request)
      .pipe(map((response) => new Word(response)));
  }

  public deleteWord(partOfSpeech: PartOfSpeech, id: string) {
    return this.httpClient.delete<void>(`${this.baseUrl}/${partOfSpeech}/${id}`);
  }
}
