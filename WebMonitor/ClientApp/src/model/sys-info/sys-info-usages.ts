import { CpuUsage } from "../cpu-usage";
import { MemoryUsage } from "../memory-usage";
import { DiskUsages } from "../disk-usage";
import { GpuUsages } from "../gpu-usage";
import { NetworkUsages } from "../network-usage";
import { ProcessList } from "../process-info";
import { ComputerInfo } from "../computer-info";

export class SysInfoUsages {
  // Keep history of usages for these values
  private _cpuUsages: CpuUsage[] = [];
  private _memoryUsages: MemoryUsage[] = [];
  private _diskUsages: DiskUsages[] = [];
  private _gpuUsages: GpuUsages[] = [];
  private _networkUsages: NetworkUsages[] = [];
  // Don't keep history of these values
  private _processInfos: ProcessList = [];
  private _computerInfo: ComputerInfo | undefined;

  /**
   * Constructor
   * @param maxHistory Maximum number of items to keep in history
   */
  constructor(private maxHistory: number) {
  }

  /**
   * Computer info getter
   */
  get computerInfo(): ComputerInfo | undefined {
    return this._computerInfo;
  }

  /**
   * Computer info setter
   * @param value Computer info
   */
  set computerInfo(value: ComputerInfo | undefined) {
    this._computerInfo = value;
  }

  /**
   * Function to update CPU usage
   * @param cpuUsage
   */
  updateCpuUsage(cpuUsage: CpuUsage) {
    this._cpuUsages.push(cpuUsage);

    if (this._cpuUsages.length > this.maxHistory) {
      this._cpuUsages.shift();
    }
  }

  /**
   * Cpu usage getter
   */
  get cpuUsage(): CpuUsage | undefined {
    return this._cpuUsages[this._cpuUsages.length - 1];
  }

  /**
   * Function to update memory usage
   * @param memoryUsage
   */
  updateMemoryUsage(memoryUsage: MemoryUsage) {
    this._memoryUsages.push(memoryUsage);

    if (this._memoryUsages.length > this.maxHistory) {
      this._memoryUsages.shift();
    }
  }

  /**
   * Memory usage getter
   */
  get memoryUsage(): MemoryUsage | undefined {
    return this._memoryUsages[this._memoryUsages.length - 1];
  }

  /**
   * Function to update disk usage
   * @param diskUsages
   */
  updateDiskUsages(diskUsages: DiskUsages) {
    this._diskUsages.push(diskUsages);

    if (this._diskUsages.length > this.maxHistory) {
      this._diskUsages.shift();
    }
  }

  /**
   * Disk usage getter
   */
  get diskUsages(): DiskUsages | undefined {
    return this._diskUsages[this._diskUsages.length - 1];
  }

  /**
   * Function to update GPU usage
   * @param gpuUsages
   */
  updateGpuUsages(gpuUsages: GpuUsages) {
    this._gpuUsages.push(gpuUsages);

    if (this._gpuUsages.length > this.maxHistory) {
      this._gpuUsages.shift();
    }
  }

  /**
   * GPU usage getter
   */
  get gpuUsages(): GpuUsages | undefined {
    return this._gpuUsages[this._gpuUsages.length - 1];
  }

  /**
   * Function to update network usage
   * @param networkUsages
   */
  updateNetworkUsages(networkUsages: NetworkUsages) {
    this._networkUsages.push(networkUsages);

    if (this._networkUsages.length > this.maxHistory) {
      this._networkUsages.shift();
    }
  }

  /**
   * Network usage getter
   */
  get networkUsages(): NetworkUsages | undefined {
    return this._networkUsages[this._networkUsages.length - 1];
  }

  /**
   * ProcessList setter
   * @param processInfos Process infos
   */
  set processInfos(processInfos: ProcessList) {
    this._processInfos = processInfos;
  }

  /**
   * Process infos getter
   */
  get processInfos(): ProcessList {
    return this._processInfos;
  }

}
