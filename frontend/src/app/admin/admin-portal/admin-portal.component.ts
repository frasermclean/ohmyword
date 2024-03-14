import { Component, OnInit } from '@angular/core';
import { RouterDataResolved } from '@ngxs/router-plugin';
import { ofActionSuccessful } from '@ngxs/store';
import { Actions } from '@ngxs/store';
import { map } from 'rxjs/operators';

@Component({
  selector: 'admin-portal',
  templateUrl: './admin-portal.component.html',
  styleUrls: ['./admin-portal.component.scss'],
})
export class AdminPortalComponent implements OnInit {
  links = [
    { label: 'Words', path: 'words', tooltip: 'Manage list of words', icon: 'group' },
    { label: 'Users', path: 'users', tooltip: 'Manage application users' },
  ];

  activeUrl$ = this.actions$.pipe(
    ofActionSuccessful(RouterDataResolved),
    map((action) => action.event.url)
  );

  constructor(private actions$: Actions) {}

  ngOnInit(): void {}
}
