import { Injectable, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  IHttpConnectionOptions,
  LogLevel,
} from '@microsoft/signalr';

export type SignalrStatus = 'disconnected' | 'connecting' | 'connected' | 'error';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private connection?: HubConnection;

  status = signal<SignalrStatus>('disconnected');
  lastError = signal<string | null>(null);

  async connect(hubUrl: string, options?: IHttpConnectionOptions): Promise<void> {
    // Ako već postoji konekcija, pokušaj da je ugasiš pre nove
    if (this.connection && this.connection.state !== HubConnectionState.Disconnected) {
      await this.disconnect();
    }

    this.status.set('connecting');
    this.lastError.set(null);

    const builder = new HubConnectionBuilder()
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information);

    // withUrl ima overload-e; pozovi odgovarajući u zavisnosti od options
    this.connection = (options ? builder.withUrl(hubUrl, options) : builder.withUrl(hubUrl)).build();

    this.connection.onreconnecting((err) => {
      this.status.set('connecting');
      this.lastError.set(err?.message ?? null);
    });

    this.connection.onreconnected(() => {
      this.status.set('connected');
      this.lastError.set(null);
    });

    this.connection.onclose((err) => {
      this.status.set('disconnected');
      this.lastError.set(err?.message ?? null);
    });

    try {
      await this.connection.start();
      this.status.set('connected');
    } catch (e) {
      const message = e instanceof Error ? e.message : 'SignalR konekcija nije uspela';
      this.status.set('error');
      this.lastError.set(message);
      // Ne bacamo dalje — UI može da prikaže status umesto da ruši aplikaciju
    }
  }

  async disconnect(): Promise<void> {
    if (!this.connection) return;
    try {
      await this.connection.stop();
    } finally {
      this.status.set('disconnected');
    }
  }

  on<T>(methodName: string, handler: (payload: T) => void): void {
    this.connection?.on(methodName, handler);
  }

  off(methodName: string, handler?: (...args: unknown[]) => void): void {
    if (!this.connection) return;
    if (handler) {
      this.connection.off(methodName, handler as (...args: any[]) => void);
      return;
    }
    this.connection.off(methodName);
  }

  invoke<T = unknown>(methodName: string, ...args: unknown[]): Promise<T> {
    if (!this.connection) {
      return Promise.reject(new Error('SignalR konekcija nije inicijalizovana'));
    }
    return this.connection.invoke<T>(methodName, ...args);
  }
}

