import { AfterViewInit, Component, ElementRef, OnInit, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { UsageGraphComponent } from "../usage-graph/usage-graph.component";
import { SysInfoService } from "../../../services/sys-info.service";
import { NetworkUsage, NetworkUsages } from 'src/model/network-usage';
import * as arrayHelpers from "../../../helpers/array-helpers";
import { DiskUsages } from 'src/model/disk-usage';
import { GpuUsages } from 'src/model/gpu-usage';

@Component({
  selector: 'app-usages',
  templateUrl: './usages.component.html',
  styleUrls: ['./usages.component.css']
})
export class UsagesComponent implements AfterViewInit {
  @ViewChild("cpuGraph") private cpuGraph!: UsageGraphComponent;
  @ViewChild("memoryGraph") private memoryGraph!: UsageGraphComponent;
  @ViewChildren("diskGraph") private diskGraphs!: QueryList<UsageGraphComponent>;
  @ViewChildren("networkGraph") private networkGraphs!: QueryList<UsageGraphComponent>;
  @ViewChildren("gpuGraph") private gpuGraphs!: QueryList<UsageGraphComponent>;

  // These are used to detect changes only
  // Actual data should be obtained from sysInfo.data
  diskUsages: DiskUsages = [];
  networkUsages: NetworkUsages = [];
  gpuUsages: GpuUsages = [];

  constructor(public sysInfo: SysInfoService) { }

  ngAfterViewInit(): void {
    this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
    this.networkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
    this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];

    this.sysInfo.onRefresh.subscribe(() => {
      // Refresh disk list if it was changed
      if (this.diskUsages.length !== this.sysInfo.data.diskUsages?.length) {
        this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
      }

      // Refresh network list if it was changed
      let newNetworkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
      if (this.networkUsages.length !== newNetworkUsages.length) {
        this.networkUsages = newNetworkUsages;
      }

      // Although it's not expected for GPUs to be hot-swappable, we still need to refresh the list
      // in cases such as when drivers are updated
      if (this.gpuUsages.length !== this.sysInfo.data.gpuUsages?.length) {
        this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];
      }

      // Update graphs
      this.cpuGraph.addValue(this.averageCpuUsage());
      this.memoryGraph.addValue(this.memoryUsagePercentage());
      // For disks, networks and GPUs we need to update all graphs from the list
      // Their names are used to match the correct graph with the correct usage
      this.diskGraphs.forEach((graph, i) => {
        const name = this.diskUsages[i]?.name ?? "";
        const utilization = this.diskUsageUtilization(name);
        graph.addValue(utilization);
      });
      this.networkGraphs.forEach((graph, i) => {
        const name = this.networkUsages[i]?.name ?? "";
        const downloadSpeed = this.networkDownloadSpeed(name);
        graph.addValue(downloadSpeed);
      });
      this.gpuGraphs.forEach((graph, i) => {
        const name = this.gpuUsages[i]?.name ?? "";
        const utilization = this.gpuUsageUtilization(name);
        graph.addValue(utilization);
      });
    })
  }

  averageCpuUsage(): number {
    return arrayHelpers.average(this.sysInfo.data.cpuUsage?.threadUsages ?? []);
  }

  averageCpuUsageHistory(): number[] {
    return this.sysInfo.data.cpuUsageHistory.map(usage =>
      arrayHelpers.average(usage.threadUsages));
  }

  memoryUsagePercentage(): number {
    return Number(this.sysInfo.data.memoryUsage?.used ?? 0n) / Number(this.sysInfo.data.memoryUsage?.total ?? 0n) * 100;
  }

  memoryUsagePercentageHistory(): number[] {
    return this.sysInfo.data.memoryUsageHistory.map(usage =>
      Number(usage.used) / Number(usage.total) * 100);
  }

  diskUsageUtilization(name: string): number {
    return this.sysInfo.data.diskUsages?.find(usage => usage.name === name)?.utilization ?? 0;
  }

  diskUsageUtilizationHistory(name: string): number[] {
    return this.sysInfo.data.diskUsagesHistory.map(usages =>
      usages.find(usage => usage.name === name)?.utilization ?? 0);
  }

  networkUsageUtilization(name: string): number {
    let usageGraph: UsageGraphComponent | undefined;

    // Network utilization needs to be relative because the maximum value is not known
    // Graphs already handle relative values, so it's easier to just get the current value from the graph than to calculate it here
    let i = 0;
    for (const usage of this.networkUsages) {
      if (usage.name === name) {
        usageGraph = this.networkGraphs.get(i);
        break;
      }
      i++;
    }

    return (usageGraph?.currentUsage ?? 0) * 100;
  }

  networkDownloadSpeed(name: string): number {
    return this.sysInfo.data.networkUsages?.find(usage => usage.name === name)?.downloadSpeed ?? 0;
  }

  networkDownloadSpeedHistory(name: string): number[] {
    return this.sysInfo.data.networkUsagesHistory.map(usages =>
      usages.find(usage => usage.name === name)?.downloadSpeed ?? 0);
  }

  networkUsageFilter(usage: NetworkUsage): boolean {
    // Filter out inactive network interfaces
    // For now inactive interfaces are those that have transferred less than 1 MB of data
    // TODO: Make this configurable
    return usage.dataDownloaded + usage.dataUploaded > 1024 * 1024;
  }

  gpuUsageUtilization(name: string): number {
    return this.sysInfo.data.gpuUsages?.find(usage => usage.name === name)?.utilization ?? 0;
  }

  gpuUsageUtilizationHistory(name: string): number[] {
    return this.sysInfo.data.gpuUsagesHistory.map(usages =>
      usages.find(usage => usage.name === name)?.utilization ?? 0);
  }

}
