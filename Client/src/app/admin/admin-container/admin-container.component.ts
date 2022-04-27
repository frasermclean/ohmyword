import { Component, OnInit } from '@angular/core';
import { MsalService } from '@azure/msal-angular';

@Component({
  selector: 'admin-container',
  templateUrl: './admin-container.component.html',
  styleUrls: ['./admin-container.component.scss']
})
export class AdminContainerComponent implements OnInit {
  constructor(private msalService: MsalService) {}

  ngOnInit(): void {}

  onLogout() {
    this.msalService.logoutRedirect({
      postLogoutRedirectUri: window.location.origin,
    });
  }
}
