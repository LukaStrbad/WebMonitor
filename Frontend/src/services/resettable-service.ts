/**
 * Interface for WebMonitor services.
 * Used to initialize and reset the service when the user logs in or out.
 */
export interface ResettableService {
    startService(): Promise<void>;
    stopService(): Promise<void>;
}
