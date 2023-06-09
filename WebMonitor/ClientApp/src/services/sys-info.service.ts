import { Inject, Injectable, effect, signal, OnDestroy } from '@angular/core';
import { firstValueFrom, Subject, Subscription } from "rxjs";
import { SysInfoUsages } from "../model/sys-info/sys-info-usages";
import { HttpClient } from "@angular/common/http";
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
  private nvidiaInitialized = false;

  private readonly apiUrl: string;
  serverVersion: string | null = null;
  refreshDelay: number = 0;
  lastSettingsUpdate = 0n;
  private supportedFeatures: SupportedFeatures | null = null;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "SysInfo/";

    // Get client IP
    firstValueFrom(this.http.get<string>(this.apiUrl + "clientIP"))
      .then(ip => this.clientIp.set(ip));

    firstValueFrom(this.http.get<string | null>(this.apiUrl + "version"))
      .then(version => this.serverVersion = version);

    this.getNvidiaRefreshSettings()
      .then(settings => {
        this.nvidiaRefreshSettings.set(settings);
        this.nvidiaInitialized = true;
      });

    // Update nvidia refresh settings when they change
    effect(async () => {
      const settings = this.nvidiaRefreshSettings();
      // Update refresh settings only after initialization to prevent double save
      if (!this.nvidiaInitialized)
        return;
      await this.updateNvidiaRefreshSettings(settings);
    })

    this.refreshLoop().then(_ => { // ignored
    });
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

  async getSupportedFeatures() : Promise<SupportedFeatures> {
    if (this.supportedFeatures == null) {
      await this.refreshSupportedFeatures();
    }

    return this.supportedFeatures!;
  }

  private async refreshLoop() {
    while (true) {
      await this.refresh();

      // Try to predict time to next refresh
      const timeToNextRefresh = Number(this.data.refreshInfo.refreshInterval) - (Number(this.data.refreshInfo.millisSinceLastRefresh) + this.refreshDelay) / 3;

      await new Promise(r => setTimeout(r, timeToNextRefresh));
    }
  }

  private async refresh() {
    // Refresh info is refreshed first to get the most accurate data
    const startTime = new Date().getTime();
    this.refreshRefreshInfo();
    // Refresh all data in parallel
    await Promise.all([
      this.checkSettingsUpdates(),
      this.refreshCpuUsage(),
      this.refreshMemoryUsage(),
      this.refreshDiskUsages(),
      this.refreshGpuUsages(),
      this.refreshNetworkUsages(),
      this.refreshProcessInfos(),
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

      // Disable NVIDIA refresh settings update to prevent infinite loop
      this.nvidiaInitialized = false;

      const nvidiaRefreshSettings = await this.getNvidiaRefreshSettings();
      const currentNvidiaRefreshSettings = this.nvidiaRefreshSettings();

      // Update NVIDIA refresh settings only if they have changed
      if (nvidiaRefreshSettings.refreshSetting !== currentNvidiaRefreshSettings.refreshSetting ||
        nvidiaRefreshSettings.nRefreshIntervals !== currentNvidiaRefreshSettings.nRefreshIntervals) {
        this.nvidiaRefreshSettings.set(nvidiaRefreshSettings);
      }
      // Re-enable NVIDIA refresh settings update
      this.nvidiaInitialized = true;
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
    const response = await firstValueFrom(
      this.http.get<CpuUsage | null>(this.apiUrl + "cpuUsage")
    );

    this.data.updateCpuUsage(response);
  }

  private async refreshMemoryUsage() {
    const response = await firstValueFrom(
      this.http.get<MemoryUsage | null>(this.apiUrl + "memoryUsage")
    );

    this.data.updateMemoryUsage(response);
  }

  private async refreshDiskUsages() {
    const response = await firstValueFrom(
      this.http.get<DiskUsages | null>(this.apiUrl + "diskUsages")
    );

    this.data.updateDiskUsages(response);
  }

  private async refreshGpuUsages() {
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
    const response = await firstValueFrom(
      this.http.get<NetworkUsages | null>(this.apiUrl + "networkUsages")
    );

    this.data.updateNetworkUsages(response);
  }

  private async refreshProcessInfos() {
    this.data.processInfos = await firstValueFrom(
      this.http.get<ProcessList | null>(this.apiUrl + "processList")
    );
  }

  private async getNvidiaRefreshSettings(): Promise<NvidiaRefreshSettings> {
    return await firstValueFrom(
      this.http.get<NvidiaRefreshSettings>(this.apiUrl + "nvidiaRefreshSettings")
    );
  }

  private async updateNvidiaRefreshSettings(settings: NvidiaRefreshSettings) {
    await firstValueFrom(this.http.post(this.apiUrl + "nvidiaRefreshSettings", settings));
  }
}
