import { HubConnectionState } from '@microsoft/signalr';

/**
 * Hub actions and events
 */
export namespace Hub {
  export class Connect {
    static readonly type = '[Game Root] Hub.Connect';
  }

  export class Disconnect {
    static readonly type = '[Game Root] Hub.Disconnect';
  }

  export class Connected {
    static readonly type = '[Game Service] Hub.Connected';
  }

  export class Disconnected {
    static readonly type = '[Game Service] Hub.Disconnected';
    constructor(public error?: any) {}
  }
}
