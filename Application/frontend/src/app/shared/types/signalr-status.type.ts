export const SIGNALR_STATUS = {
  DISCONNECTED: 'disconnected',
  CONNECTING: 'connecting',
  CONNECTED: 'connected',
  ERROR: 'error',
} as const;

export type SignalrStatus =
  (typeof SIGNALR_STATUS)[keyof typeof SIGNALR_STATUS];
