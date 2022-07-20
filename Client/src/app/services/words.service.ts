import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { map } from 'rxjs/operators';
import { environment } from 'src/environments/environment';
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
}
