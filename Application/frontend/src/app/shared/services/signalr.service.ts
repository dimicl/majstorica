import { Injectable, signal } from '@angular/core';
import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  IHttpConnectionOptions,
  LogLevel,
} from '@microsoft/signalr';
import { SIGNALR_STATUS, SignalrStatus } from '../types';

@Injectable({
  providedIn: 'root',
})
export class SignalrService {
  private connection?: HubConnection;

  status = signal<SignalrStatus>(SIGNALR_STATUS.DISCONNECTED);
  lastError = signal<string | null>(null);

  async connect(
    hubUrl: string,
    options?: IHttpConnectionOptions
  ): Promise<void> {
    // Ako već postoji konekcija, pokušaj da je ugasiš pre nove
    if (
      this.connection &&
      this.connection.state !== HubConnectionState.Disconnected
    ) {
      await this.disconnect();
    }

    this.status.set(SIGNALR_STATUS.CONNECTING);
    this.lastError.set(null);

    if (options?.accessTokenFactory) {
      const raw = options.accessTokenFactory();
      const token = typeof raw === 'string' ? raw : await raw;
      if (!token?.trim()) {
        this.status.set(SIGNALR_STATUS.ERROR);
        this.lastError.set('Nema JWT tokena za SignalR.');
        return;
      }
    }

    const builder = new HubConnectionBuilder()
      .withAutomaticReconnect()
      .configureLogging(LogLevel.Information);

    // withUrl ima overload-e; pozovi odgovarajući u zavisnosti od options
    this.connection = (
      options ? builder.withUrl(hubUrl, options) : builder.withUrl(hubUrl)
    ).build();

    this.connection.onreconnecting((err) => {
      this.status.set(SIGNALR_STATUS.CONNECTING);
      this.lastError.set(err?.message ?? null);
    });

    this.connection.onreconnected(() => {
      this.status.set(SIGNALR_STATUS.CONNECTED);
      this.lastError.set(null);
    });

    this.connection.onclose((err) => {
      this.status.set(SIGNALR_STATUS.DISCONNECTED);
      this.lastError.set(err?.message ?? null);
    });

    try {
      await this.connection.start();
      this.status.set(SIGNALR_STATUS.CONNECTED);
    } catch (e) {
      const message =
        e instanceof Error ? e.message : 'SignalR konekcija nije uspela';
      this.status.set(SIGNALR_STATUS.ERROR);
      this.lastError.set(message);
      // Ne bacamo dalje — UI može da prikaže status umesto da ruši aplikaciju
    }
  }

  async disconnect(): Promise<void> {
    if (!this.connection) return;
    try {
      await this.connection.stop();
    } finally {
      this.status.set(SIGNALR_STATUS.DISCONNECTED);
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
      return Promise.reject(
        new Error('SignalR konekcija nije inicijalizovana')
      );
    }
    return this.connection.invoke<T>(methodName, ...args);
  }
}
