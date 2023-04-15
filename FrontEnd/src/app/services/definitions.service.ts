import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { environment } from '@environment';
import { Definition } from '@models/definition.model';
import { DefinitionResponse } from '@models/responses/definition-response';
import { map } from 'rxjs/operators';

@Injectable({
  providedIn: 'root',
})
export class DefinitionsService {
  private readonly baseUrl = `https://${environment.apiHost}/api/definitions`;

  constructor(private httpClient: HttpClient) {}

  /**
   * Fetches dictionary definitions for a word.
   * @param wordId The word to search for.
   * @returns An observable of array of definitions.
   */
  public getDefinitions(wordId: string) {
    return this.httpClient
      .get<DefinitionResponse[]>(`${this.baseUrl}/${wordId}`)
      .pipe(map((responses) => responses.map((response) => new Definition(response))));
  }
}
