import { Inject, Injectable, OnDestroy } from '@angular/core';
import { firstValueFrom, interval, Subject } from "rxjs";
import { SysInfoUsages } from "../model/sys-info/sys-info-usages";
import { HttpClient } from "@angular/common/http";
import { CpuUsage } from "../model/cpu-usage";
import { MemoryUsage } from "../model/memory-usage";
import { DiskUsages } from "../model/disk-usage";
import { GpuUsages } from "../model/gpu-usage";
import { NetworkUsages } from "../model/network-usage";
import { ProcessList } from "../model/process-info";
import { ComputerInfo } from "../model/computer-info";

@Injectable({
  providedIn: 'root'
})
export class SysInfoService implements OnDestroy {
  onRefresh = new Subject<void>();
  data = new SysInfoUsages(60);

  private readonly apiUrl: string;
  private readonly loopSubscription: any;

  constructor(
    private http: HttpClient,
    @Inject('BASE_URL') baseUrl: string
  ) {
    this.apiUrl = baseUrl + "SysInfo/";

    // Computer info should not be refreshed every time
    this.refreshComputerInfo().then(_ => {
    });

    this.loopSubscription = interval(1000).subscribe(async () => await this.refresh());
  }

  ngOnDestroy(): void {
    this.loopSubscription.unsubscribe();
  }

  private async refresh() {
    // Refresh all data in parallel
    await Promise.all([
      this.refreshCpuUsage(),
      this.refreshMemoryUsage(),
      this.refreshDiskUsages(),
      this.refreshGpuUsages(),
      this.refreshNetworkUsages(),
      this.refreshProcessInfos()
    ]);
    // Notify subscribers that data has been refreshed
    this.onRefresh.next();
  }

  private async refreshComputerInfo() {
    this.data.computerInfo = await firstValueFrom(
      this.http.get<ComputerInfo>(this.apiUrl + "computerInfo")
    );
  }

  private async refreshCpuUsage() {
    const response = await firstValueFrom(
      this.http.get<CpuUsage>(this.apiUrl + "cpuUsage")
    );
    this.data.updateCpuUsage(response);
  }

  private async refreshMemoryUsage() {
    const response = await firstValueFrom(
      this.http.get<MemoryUsage>(this.apiUrl + "memoryUsage")
    );
    this.data.updateMemoryUsage(response);
  }

  private async refreshDiskUsages() {
    const response = await firstValueFrom(
      this.http.get<DiskUsages>(this.apiUrl + "diskUsages")
    );
    this.data.updateDiskUsages(response);
  }

  private async refreshGpuUsages() {
    const response = await firstValueFrom(
      this.http.get<GpuUsages>(this.apiUrl + "gpuUsages")
    );
    this.data.updateGpuUsages(response);
  }

  private async refreshNetworkUsages() {
    const response = await firstValueFrom(
      this.http.get<NetworkUsages>(this.apiUrl + "networkUsages")
    );
    this.data.updateNetworkUsages(response);
  }

  private async refreshProcessInfos() {
    this.data.processInfos = await firstValueFrom(
      this.http.get<ProcessList>(this.apiUrl + "processList")
    );
  }
}
