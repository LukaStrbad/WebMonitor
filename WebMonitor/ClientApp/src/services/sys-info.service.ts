import { EventEmitter, Inject, Injectable, signal } from '@angular/core';
import { catchError, firstValueFrom, Subject, throwError } from "rxjs";
import { SysInfoUsages } from "../model/sys-info/sys-info-usages";
import { HttpClient, HttpErrorResponse } from "@angular/common/http";
import { CpuUsage } from "../model/cpu-usage";
import { MemoryUsage } from "../model/memory-usage";
import { DiskUsages } from "../model/disk-usage";
import { GpuUsages } from "../model/gpu-usage";
import { NetworkUsages } from "../model/network-usage";
import { ProcessList } from "../model/process-info";
import { ComputerInfo } from "../model/computer-info";
import { RefreshInformation } from 'src/model/refresh-information';
import { NvidiaRefreshSetting, NvidiaRefreshSettings } from 'src/model/nvidia-refresh-setting';
import { SupportedFeatures } from "../model/supported-features";
import { BatteryInfo } from "../model/battery-info";
import { ExtendedProcessInfo } from "../model/extended-process-info";
import { UserService } from './user.service';

@Injectable({
  providedIn: 'root'
})
export class SysInfoService {
  onRefresh = new Subject<void>();
  data = new SysInfoUsages(60);
  /**
   * IP address of the connected device
   */
  clientIp = signal<string | null>(null);
  nvidiaRefreshSettings = signal<NvidiaRefreshSettings>({
    refreshSetting: NvidiaRefreshSetting.Enabled,
    nRefreshIntervals: 10
  });

  private readonly apiUrl: string;
  serverVersion: string | null = null;
  refreshDelay: number = 0;
  lastSettingsUpdate = 0n;
  private supportedFeatures: SupportedFeatures | null = null;
  private stopRefresh = false;
  private refreshTimeout: any;

  errorEmitter = new EventEmitter<[SysInfoError, string]>();

  constructor(
    private http: HttpClient,
    private userService: UserService,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "SysInfo/";

    if (userService.authorized()) {
      this.startService();
    }
  }

  /**
   * Function that starts sys info service when user logs in
   */
  startService() {
    // Reset fields
    this.stopRefresh = false;
    this.data = new SysInfoUsages(60);
    this.clientIp.set(null);
    this.nvidiaRefreshSettings.set({
      refreshSetting: NvidiaRefreshSetting.Enabled,
      nRefreshIntervals: 10
    });
    this.serverVersion = null;
    this.refreshDelay = 0;
    this.lastSettingsUpdate = 0n;
    this.supportedFeatures = null;


    // Get client IP
    firstValueFrom(this.http.get<string>(this.apiUrl + "clientIP"))
      .then(ip => this.clientIp.set(ip));

    firstValueFrom(this.http.get<string | null>(this.apiUrl + "version"))
      .then(version => this.serverVersion = version);

    this.getNvidiaRefreshSettings()
      .then(settings => {
        console.log("NVIDIA refresh settings", settings);
        this.nvidiaRefreshSettings.set(settings);
      });

    this.refreshLoop().then(_ => { // ignored
    });
  }

  /**
   * Function that stops sysinfo service when user logs out
   */
  stopService() {
    this.stopRefresh = true;
    clearTimeout(this.refreshTimeout ?? undefined);
  }

  /**
   * Method that waits for the first refresh to complete
   * or returns immediately if the first refresh has already completed
   * @returns Computer info
   */
  async getComputerInfo(): Promise<ComputerInfo> {
    if (this.data.computerInfo == null) {
      await this.refreshComputerInfo();
    }

    return this.data.computerInfo!;
  }

  async getSupportedFeatures(): Promise<SupportedFeatures> {
    if (this.supportedFeatures == null) {
      await this.refreshSupportedFeatures();
    }

    return this.supportedFeatures!;
  }

  private async refreshLoop() {
    while (true) {
      if (this.stopRefresh) {
        console.log("Refresh stopped");
        return;
      }

      if (this.userService.user) {
        try {
          await this.refresh();
        } catch (e) {
        }
      }

      // Try to predict time to next refresh
      const timeToNextRefresh = Number(this.data.refreshInfo.refreshInterval) - (Number(this.data.refreshInfo.millisSinceLastRefresh) + this.refreshDelay) / 3;

      await new Promise(r => setTimeout(r, timeToNextRefresh));
    }
  }

  private async refresh() {
    // Refresh info is refreshed first to get the most accurate data
    const startTime = new Date().getTime();
    this.refreshRefreshInfo();
    this.userService.refreshUser();
    // Refresh all data in parallel
    await Promise.all([
      this.checkSettingsUpdates(),
      this.refreshCpuUsage(),
      this.refreshMemoryUsage(),
      this.refreshDiskUsages(),
      this.refreshGpuUsages(),
      this.refreshNetworkUsages(),
      this.refreshProcessInfos(),
      this.refreshBatteryInfo()
    ]);
    // Notify subscribers that data has been refreshed
    this.onRefresh.next();
    this.refreshDelay = new Date().getTime() - startTime;
  }

  private async refreshRefreshInfo() {
    this.data.refreshInfo = await firstValueFrom(
      this.http.get<RefreshInformation>(this.apiUrl + "refreshInfo")
    );
  }

  private async checkSettingsUpdates() {
    const time = await firstValueFrom(
      this.http.get<bigint>(this.apiUrl + "settingsUpdateTime")
    );

    if (time > this.lastSettingsUpdate) {
      this.lastSettingsUpdate = time;
      // Refresh interval is being updated constantly, so it is not checked here

      const nvidiaRefreshSettings = await this.getNvidiaRefreshSettings();
      const currentNvidiaRefreshSettings = this.nvidiaRefreshSettings();

      // Update NVIDIA refresh settings only if they have changed
      if (nvidiaRefreshSettings.refreshSetting !== currentNvidiaRefreshSettings.refreshSetting ||
        nvidiaRefreshSettings.nRefreshIntervals !== currentNvidiaRefreshSettings.nRefreshIntervals) {
        this.nvidiaRefreshSettings.set(nvidiaRefreshSettings);
      }
    }
  }

  async updateRefreshInterval(interval: number) {
    const result = await firstValueFrom(
      this.http.post(this.apiUrl + "refreshInterval", interval, { observe: "response" })
    );

    if (result.ok) {
      this.data.refreshInfo.refreshInterval = interval;
    }

    return result.ok;
  }

  private async refreshComputerInfo() {
    this.data.computerInfo = await firstValueFrom(
      this.http.get<ComputerInfo>(this.apiUrl + "computerInfo")
    );
  }

  private async refreshSupportedFeatures() {
    this.supportedFeatures = await firstValueFrom(
      this.http.get<SupportedFeatures>(this.apiUrl + "supportedFeatures")
    );
  }

  private async refreshCpuUsage() {
    if (!this.userService.user?.allowedFeatures.cpuUsage) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<CpuUsage | null>(this.apiUrl + "cpuUsage")
    );

    this.data.updateCpuUsage(response);
  }

  private async refreshMemoryUsage() {
    if (!this.userService.user?.allowedFeatures.memoryUsage) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<MemoryUsage | null>(this.apiUrl + "memoryUsage")
    );

    this.data.updateMemoryUsage(response);
  }

  private async refreshDiskUsages() {
    if (!this.userService.user?.allowedFeatures.diskUsage) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<DiskUsages | null>(this.apiUrl + "diskUsages")
    );

    this.data.updateDiskUsages(response);
  }

  private async refreshGpuUsages() {
    if (!this.userService.user?.allowedFeatures.gpuUsage) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<GpuUsages | null>(this.apiUrl + "gpuUsages")
    );

    if (this.nvidiaRefreshSettings().refreshSetting === NvidiaRefreshSetting.Disabled && response != null) {
      this.data.updateGpuUsages(response.filter(g => g.manufacturer !== "NVIDIA"));
    } else {
      this.data.updateGpuUsages(response);
    }
  }

  private async refreshNetworkUsages() {
    if (!this.userService.user?.allowedFeatures.networkUsage) {
      return;
    }

    const response = await firstValueFrom(
      this.http.get<NetworkUsages | null>(this.apiUrl + "networkUsages")
    );

    this.data.updateNetworkUsages(response);
  }

  private async refreshProcessInfos() {
    if (!this.userService.user?.allowedFeatures.processes) {
      return;
    }

    this.data.processInfos = await firstValueFrom(
      this.http.get<ProcessList | null>(this.apiUrl + "processList")
    );
  }

  private async getNvidiaRefreshSettings(): Promise<NvidiaRefreshSettings> {
    return await firstValueFrom(
      this.http.get<NvidiaRefreshSettings>(this.apiUrl + "nvidiaRefreshSettings")
    );
  }

  async updateNvidiaRefreshSettings() {
    if (!this.userService.user?.allowedFeatures.nvidiaRefreshSettings) {
      return;
    }

    // Print stack trace
    console.trace();

    await firstValueFrom(this.http.post(this.apiUrl + "nvidiaRefreshSettings", this.nvidiaRefreshSettings()));
  }

  private async refreshBatteryInfo() {
    if (!this.userService.user?.allowedFeatures.batteryInfo) {
      return;
    }

    // Refresh battery info only if it has not been refreshed for a long time
    if (this.data.refreshInfo.millisSinceLastRefresh2 < (this.data.refreshInfo.refreshInterval * 5) * 0.9)
      return;

    this.data.batteryInfo = await firstValueFrom(
      this.http.get<BatteryInfo | null>(this.apiUrl + "batteryInfo")
    );
  }

  async getExtendedProcessInfo(pid: number): Promise<ExtendedProcessInfo> {
    return await firstValueFrom(
      this.http.get<ExtendedProcessInfo>(`${this.apiUrl}extendedProcessInfo?pid=${pid}`)
        .pipe(catchError((err: HttpErrorResponse) => {
          if (err.status === 401) {
            this.errorEmitter.emit([SysInfoError.ExtendedProcessInfo, "Access Denied"]);
          } else if (err.error) {
            this.errorEmitter.emit([SysInfoError.ExtendedProcessInfo, err.error]);
          } else {
            this.errorEmitter.emit([SysInfoError.ExtendedProcessInfo, err.message]);
          }
          return throwError(() => err.error);
        }))
    );
  }
}

export enum SysInfoError {
  ExtendedProcessInfo
}
