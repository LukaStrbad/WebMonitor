import {
  AfterViewInit,
  Component,
  OnDestroy,
  QueryList,
  Signal,
  ViewChild,
  ViewChildren,
  computed
} from '@angular/core';
import { UsageGraphComponent } from "../../components/usage-graph/usage-graph.component";
import { SysInfoService } from "../../../services/sys-info.service";
import { NetworkUsage, NetworkUsages } from 'src/model/network-usage';
import * as arrayHelpers from "../../../helpers/array-helpers";
import { DiskUsage, DiskUsages } from 'src/model/disk-usage';
import { GpuUsages } from 'src/model/gpu-usage';
import * as numberHelpers from "../../../helpers/number-helpers";
import { replaceValues } from "../../../helpers/object-helpers";
import { Subscription } from 'rxjs';
import { AppSettingsService, GraphColors } from 'src/services/app-settings.service';
import { SupportedFeatures } from "../../../model/supported-features";

@Component({
  selector: 'app-usages',
  templateUrl: './usages.component.html',
  styleUrls: ['./usages.component.css']
})
export class UsagesComponent implements AfterViewInit, OnDestroy {
  @ViewChild("cpuGraph") private cpuGraph!: UsageGraphComponent;
  @ViewChild("memoryGraph") private memoryGraph!: UsageGraphComponent;
  @ViewChildren("diskGraph") private diskGraphs!: QueryList<UsageGraphComponent>;
  @ViewChildren("networkGraph") private networkGraphs!: QueryList<UsageGraphComponent>;
  @ViewChildren("gpuGraph") private gpuGraphs!: QueryList<UsageGraphComponent>;
  graphColors: Signal<GraphColors>;

  // These are used to detect changes only
  // Actual data should be obtained from sysInfo.data
  diskUsages: DiskUsages = [];
  networkUsages: NetworkUsages = [];
  gpuUsages: GpuUsages = [];

  refreshSubscription: Subscription | undefined;
  supportedFeatures?: SupportedFeatures;

  constructor(
    public sysInfo: SysInfoService,
    appSettings: AppSettingsService
  ) {
    this.graphColors = computed(() => appSettings.settings().graphColors);

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
    this.networkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
    this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];

    this.refreshSubscription = this.sysInfo.onRefresh.subscribe(() => {
      // Refresh disk list if it was changed
      if (this.diskUsages.length !== this.sysInfo.data.diskUsages?.length) {
        this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
      } else {
        // Refresh values
        this.sysInfo.data.diskUsages?.forEach((diskUsage, i) => {
          // We can't just assign the new value to the old one because it will break the reference
          // and the progress bars will be recreated, starting from zero every update
          replaceValues(this.diskUsages[i], diskUsage);
        });
      }

      // Refresh network list if it was changed
      let newNetworkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
      if (this.networkUsages.length !== newNetworkUsages.length) {
        this.networkUsages = newNetworkUsages;
      } else {
        // Refresh values
        newNetworkUsages.forEach((networkUsage, i) => {
          replaceValues(this.networkUsages[i], networkUsage);
        });
      }

      // Refresh GPU list if there was a driver change or NVIDIA monitoring was enabled/disabled
      if (this.gpuUsages.length !== this.sysInfo.data.gpuUsages?.length) {
        this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];
      } else {
        // Refresh values
        this.sysInfo.data.gpuUsages?.forEach((gpuUsage, i) => {
          replaceValues(this.gpuUsages[i], gpuUsage);
        });
      }

      // Update graphs
      this.cpuGraph.addValue(this.averageCpuUsage());
      this.memoryGraph.addValue(this.memoryUsagePercentage());
      // For disks, networks and GPUs we need to update all graphs from the list
      // Their names are used to match the correct graph with the correct usage
      this.diskGraphs.forEach((graph, i) => {
        const utilization = this.diskUsages[i].utilization;
        graph.addValue(utilization);
      });
      this.networkGraphs.forEach((graph, i) => {
        const downloadSpeed = this.networkUsages[i].downloadSpeed;
        const uploadSpeed = this.networkUsages[i].uploadSpeed;
        graph.addValues(downloadSpeed, uploadSpeed);
      });
      this.gpuGraphs.forEach((graph, i) => {
        const utilization = this.gpuUsages[i].utilization;
        graph.addValue(utilization);
      });
    })
  }

  bytes(value: number | bigint | undefined): string {
    return numberHelpers.toByteString(value ?? 0);
  }

  byteRatio(
    value1: number | bigint | undefined,
    value2: number | bigint | undefined
  ): string {
    return numberHelpers.toByteStringRatio(value1 ?? 0, value2 ?? 0);
  }

  bits(value: number | bigint | undefined): string {
    return numberHelpers.toByteString(value ?? 0, new numberHelpers.MemoryByteOptions(true, true));
  }

  averageCpuUsage(): number {
    return arrayHelpers.average(this.sysInfo.data.cpuUsage?.threadUsages ?? []);
  }

  averageCpuUsageHistory(): number[] {
    return this.sysInfo.data.cpuUsageHistory
      .filter(usage => usage != null)
      .map(usage => arrayHelpers.average(usage!.threadUsages));
  }

  memoryUsagePercentage(): number {
    return Number(this.sysInfo.data.memoryUsage?.used ?? 0n) / Number(this.sysInfo.data.memoryUsage?.total ?? 0n) * 100;
  }

  memoryUsagePercentageHistory(): number[] {
    return this.sysInfo.data.memoryUsageHistory
      .filter(usage => usage != null)
      .map(usage => Number(usage!.used) / Number(usage!.total) * 100);
  }

  diskUsageUtilizationHistory(diskUsage: DiskUsage): number[] {
    return this.sysInfo.data.diskUsagesHistory
      .filter(usages => usages != null)
      .map(usages => usages!.find(usage => usage.name === diskUsage.name)?.utilization ?? 0);
  }

  diskSpeeds(diskUsage: DiskUsage): string {
    const usage = this.sysInfo.data.diskUsages?.find(usage => usage.name === diskUsage.name);
    const readSpeed = this.bytes(usage?.readSpeed);
    const writeSpeed = this.bytes(usage?.writeSpeed);

    return `R: ${readSpeed}/s | W: ${writeSpeed}/s`;
  }

  networkUsageUtilization(networkUsage: NetworkUsage): number {
    let usageGraph: UsageGraphComponent | undefined;

    // Network utilization needs to be relative because the maximum value is not known
    // Graphs already handle relative values, so it's easier to just get the current value from the graph than to calculate it here
    let i = 0;
    for (const usage of this.networkUsages) {
      if (usage.name === networkUsage.name) {
        usageGraph = this.networkGraphs.get(i);
        break;
      }
      i++;
    }

    return (usageGraph?.currentUsage ?? 0) * 100;
  }

  networkDownloadSpeedHistory(networkUsage: NetworkUsage): number[] {
    return this.sysInfo.data.networkUsagesHistory
      .filter(usages => usages != null)
      .map(usages => usages!.find(usage => usage.name === networkUsage.name)?.downloadSpeed ?? 0);
  }

  networkUploadSpeedHistory(networkUsage: NetworkUsage): number[] {
    return this.sysInfo.data.networkUsagesHistory
      .filter(usages => usages != null)
      .map(usages => usages!.find(usage => usage.name === networkUsage.name)?.uploadSpeed ?? 0);
  }

  networkUsageFilter(usage: NetworkUsage): boolean {
    // Filter out inactive network interfaces
    // For now inactive interfaces are those that have transferred less than 1 MB of data
    // TODO: Make this configurable
    return usage.dataDownloaded + usage.dataUploaded > 1024 * 1024;
  }

  networkSpeeds(networkUsage: NetworkUsage): string {
    const usage = this.sysInfo.data.networkUsages?.find(usage => usage.name === networkUsage.name);
    const downloadSpeed = this.bits(usage?.downloadSpeed);
    const uploadSpeed = this.bits(usage?.uploadSpeed);

    return `↓: ${downloadSpeed}/s | ↑: ${uploadSpeed}/s`;
  }

  gpuUsageUtilizationHistory(name: string): number[] {
    return this.sysInfo.data.gpuUsagesHistory
      .filter(usages => usages != null)
      .map(usages => usages!.find(usage => usage.name === name)?.utilization ?? 0);
  }

}
