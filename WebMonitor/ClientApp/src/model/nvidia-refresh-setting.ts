export enum NvidiaRefreshSetting {
    Enabled = 0,
    PartiallyDisabled = 1,
    Disabled = 2,
    LongerInterval = 3
}

export interface NvidiaRefreshSettings {
    refreshSetting: NvidiaRefreshSetting;
    nRefreshIntervals: number;
}
