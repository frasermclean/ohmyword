import { Component } from '@angular/core';
import { GameService } from './services/game.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
})
export class AppComponent {
  isRegistered$ = this.gameService.register();
  constructor(private gameService: GameService) {}
}
