import { Role } from "../models/role.enum";

export namespace Auth {
  export class Initialize {
    static readonly type = '[Auth] Initialize';
  }

  export class Login {
    static readonly type = '[Toolbar] Login';
  }

  export class Logout {
    static readonly type = '[Toolbar] Logout';
  }

  export class LoggedIn {
    static readonly type = '[Auth Service] Logged In';
    constructor(public displayName: string, public role: Role) {}
  }

  export class LoggedOut {
    static readonly type = '[Auth Service] Logged Out';
  }
}
