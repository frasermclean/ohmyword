import { Component, OnInit } from '@angular/core';

@Component({
  selector: 'admin-portal',
  templateUrl: './admin-portal.component.html',
  styleUrls: ['./admin-portal.component.scss'],
})
export class AdminPortalComponent implements OnInit {
  links = [
    { label: 'Words', path: 'words', tooltip: 'Manage list of words', icon: 'group' },
    { label: 'Users', path: 'users', tooltip: 'Manage application users' },
  ]
  activeLink = this.links[0];

  constructor() {}

  ngOnInit(): void {}
}
