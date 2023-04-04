import { Role } from '../models/role.enum';

export namespace Auth {
  export class Login {
    static readonly type = '[Toolbar] Login';
  }

  export class Logout {
    static readonly type = '[Toolbar] Logout';
  }

  export class Complete {
    static readonly type = '[Auth Service] Complete';
    constructor(public displayName: string, public role: Role) {}
  }
}
