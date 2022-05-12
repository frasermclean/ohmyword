import { DOCUMENT } from '@angular/common';
import { Component, Inject } from '@angular/core';

@Component({
  selector: 'app-root',
  template: `
    <app-toolbar></app-toolbar>
    <router-outlet></router-outlet>
  `,
})
export class AppComponent {
  constructor(@Inject(DOCUMENT) private document: Document) {
    console.log(document);
    document
  }
}
