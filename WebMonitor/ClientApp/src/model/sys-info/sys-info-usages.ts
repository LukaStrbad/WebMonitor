import { CpuUsage } from "../cpu-usage";
import { MemoryUsage } from "../memory-usage";
import { DiskUsages } from "../disk-usage";
import { GpuUsages } from "../gpu-usage";
import { NetworkUsages } from "../network-usage";
import { ProcessList } from "../process-info";
import { ComputerInfo } from "../computer-info";
import { RefreshInformation } from "../refresh-information";

export class SysInfoUsages {
  // Data age in milliseconds
  private _millisSinceRefresh: RefreshInformation = { millisSinceLastRefresh: 0, refreshInterval: 0 };
  // Keep history of usages for these values
  private _cpuUsages: (CpuUsage | null)[] = [];
  private _memoryUsages: (MemoryUsage | null)[] = [];
  private _diskUsages: (DiskUsages | null)[] = [];
  private _gpuUsages: (GpuUsages | null)[] = [];
  private _networkUsages: (NetworkUsages | null)[] = [];
  // Don't keep history of these values
  private _processInfos: (ProcessList | null) = [];
  private _computerInfo: ComputerInfo | null = null;

  /**
   * Constructor
   * @param maxHistory Maximum number of items to keep in history
   */
  constructor(private maxHistory: number) {
  }

  /**
   * Information about refreshes
   */
  get refreshInfo(): RefreshInformation {
    return this._millisSinceRefresh;
  }

  set refreshInfo(value: RefreshInformation) {
    this._millisSinceRefresh = value;
  }

  /**
   * Computer info getter
   */
  get computerInfo(): ComputerInfo | null {
    return this._computerInfo;
  }

  /**
   * Computer info setter
   * @param value Computer info
   */
  set computerInfo(value: ComputerInfo | null) {
    this._computerInfo = value;
  }

  /**
   * Function to update CPU usage
   * @param cpuUsage
   */
  updateCpuUsage(cpuUsage: CpuUsage | null) {
    this._cpuUsages.push(cpuUsage);

    if (this._cpuUsages.length > this.maxHistory) {
      this._cpuUsages.shift();
    }
  }

  /**
   * Cpu usage getter
   */
  get cpuUsage(): CpuUsage | null {
    return this._cpuUsages[this._cpuUsages.length - 1];
  }

  /**
   * Cpu usage history getter
   */
  get cpuUsageHistory(): (CpuUsage | null)[] {
    return this._cpuUsages;
  }

  /**
   * Function to update memory usage
   * @param memoryUsage
   */
  updateMemoryUsage(memoryUsage: MemoryUsage  | null) {
    this._memoryUsages.push(memoryUsage);

    if (this._memoryUsages.length > this.maxHistory) {
      this._memoryUsages.shift();
    }
  }

  /**
   * Memory usage getter
   */
  get memoryUsage(): MemoryUsage | null {
    return this._memoryUsages[this._memoryUsages.length - 1];
  }

  /**
   * Memory usage history getter
   */
  get memoryUsageHistory(): (MemoryUsage | null)[] {
    return this._memoryUsages;
  }

  /**
   * Function to update disk usage
   * @param diskUsages
   */
  updateDiskUsages(diskUsages: DiskUsages | null) {
    this._diskUsages.push(diskUsages);

    if (this._diskUsages.length > this.maxHistory) {
      this._diskUsages.shift();
    }
  }

  /**
   * Disk usage getter
   */
  get diskUsages(): DiskUsages | null {
    return this._diskUsages[this._diskUsages.length - 1];
  }

  /**
   * Disk usage history getter
   */
  get diskUsagesHistory(): (DiskUsages | null)[] {
    return this._diskUsages;
  }

  /**
   * Function to update GPU usage
   * @param gpuUsages
   */
  updateGpuUsages(gpuUsages: GpuUsages | null) {
    this._gpuUsages.push(gpuUsages);

    if (this._gpuUsages.length > this.maxHistory) {
      this._gpuUsages.shift();
    }
  }

  /**
   * GPU usage getter
   */
  get gpuUsages(): GpuUsages | null {
    return this._gpuUsages[this._gpuUsages.length - 1];
  }

  /**
   * GPU usage history getter
   */
  get gpuUsagesHistory(): (GpuUsages | null)[] {
    return this._gpuUsages;
  }

  /**
   * Function to update network usage
   * @param networkUsages
   */
  updateNetworkUsages(networkUsages: NetworkUsages | null) {
    this._networkUsages.push(networkUsages);

    if (this._networkUsages.length > this.maxHistory) {
      this._networkUsages.shift();
    }
  }

  /**
   * Network usage getter
   */
  get networkUsages(): NetworkUsages | null {
    return this._networkUsages[this._networkUsages.length - 1];
  }

  /**
   * Network usage history getter
   */
  get networkUsagesHistory(): (NetworkUsages | null)[] {
    return this._networkUsages;
  }

  /**
   * ProcessList setter
   * @param processInfos Process infos
   */
  set processInfos(processInfos: ProcessList | null) {
    this._processInfos = processInfos;
  }

  /**
   * Process infos getter
   */
  get processInfos(): ProcessList | null {
    return this._processInfos;
  }

}
