import { Injectable } from '@angular/core';
import { PlayerData } from '@models/player-data.model';
import { v4 as uuidv4 } from 'uuid';

@Injectable({
  providedIn: 'root',
})
export class StorageService {
  /**
   * Attempt to get player data from local storage
   * @returns Player data if found, otherwise null
   */
  public getPlayerData() {
    const result = localStorage.getItem('playerData');
    return result ? (JSON.parse(result) as PlayerData) : null;
  }

  /**
   * Save player data to local storage
   * @param data Player data to save to local storage
   */
  public setPlayerData(data: PlayerData) {
    localStorage.setItem('playerData', JSON.stringify(data));
  }

  /**
   * Create a new player data object
   * @returns A new player data object
   */
  public createPlayerData() {
    return {
      playerId: uuidv4(),
      score: 0,
      registrationCount: 0,
    } as PlayerData;
  }
}
