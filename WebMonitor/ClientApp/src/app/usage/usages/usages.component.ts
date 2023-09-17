import {
  AfterViewInit,
  Component,
  OnDestroy,
  QueryList,
  Signal,
  ViewChild,
  ViewChildren,
  computed, OnInit, ChangeDetectorRef
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
import { AllowedFeatures } from 'src/model/allowed-features';
import { UserService } from 'src/services/user.service';

@Component({
  selector: 'app-usages',
  templateUrl: './usages.component.html',
  styleUrls: ['./usages.component.css']
})
export class UsagesComponent implements AfterViewInit, OnInit, OnDestroy {
  @ViewChild("cpuGraph") private cpuGraph?: UsageGraphComponent;
  @ViewChild("memoryGraph") private memoryGraph?: UsageGraphComponent;
  @ViewChildren("diskGraph") private diskGraphs?: QueryList<UsageGraphComponent>;
  @ViewChildren("networkGraph") private networkGraphs?: QueryList<UsageGraphComponent>;
  @ViewChildren("gpuGraph") private gpuGraphs?: QueryList<UsageGraphComponent>;
  initializedGraphs = {
    cpu: false,
    memory: false,
    disks: false,
    networks: false,
    gpus: false
  };

  graphColors: Signal<GraphColors>;

  // These are used to detect changes only
  // Actual data should be obtained from sysInfo.data
  diskUsages: DiskUsages = [];
  networkUsages: NetworkUsages = [];
  gpuUsages: GpuUsages = [];

  refreshSubscription: Subscription | undefined;
  supportedFeatures?: SupportedFeatures;
  allowedFeatures?: AllowedFeatures;

  // Using the field instead of calling the function directly stops the ExpressionChangedAfterItHasBeenCheckedError
  networkUsageUtilizations: number[] = [];

  constructor(
    public sysInfo: SysInfoService,
    appSettings: AppSettingsService,
    userService: UserService,
    private changeDetector: ChangeDetectorRef
  ) {
    this.graphColors = computed(() => appSettings.settings().graphColors);

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);

    userService.requireUser()
      .then(user => this.allowedFeatures = user.allowedFeatures);


  }

  ngOnInit(): void {
    this.networkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
    // Bind the function to "this" so it can be used in the template
    this.networkUsageUtilizations = this.networkUsages.map(this._networkUsageUtilization.bind(this));
    this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
    this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];
    // Force change detection to instantly show all panels
    this.changeDetector.detectChanges();
  }

  ngOnDestroy(): void {
    this.refreshSubscription?.unsubscribe();
  }

  ngAfterViewInit(): void {
    // Initialize graphs with previous values to avoid showing empty graphs before the next refresh
    this.onRefresh();
    this.refreshSubscription = this.sysInfo.onRefresh.subscribe(this.onRefresh.bind(this));
  }

  private onRefresh() {
    if (this.showDiskUsage()) {
      // Refresh disk list if it was changed
      if (this.diskUsages.length !== this.sysInfo.data.diskUsages?.length) {
        this.diskUsages = [...this.sysInfo.data.diskUsages ?? []];
        this.initializedGraphs.disks = false;
      } else {
        // Refresh values
        this.sysInfo.data.diskUsages?.forEach((diskUsage, i) => {
          // We can't just assign the new value to the old one because it will break the reference
          // and the progress bars will be recreated, starting from zero every update
          replaceValues(this.diskUsages[i], diskUsage);
        });
      }
    }

    if (this.showNetworkUsage()) {
      // Refresh network list if it was changed
      let newNetworkUsages = [...this.sysInfo.data.networkUsages ?? []].filter(this.networkUsageFilter);
      if (this.networkUsages.length !== newNetworkUsages.length) {
        this.networkUsages = newNetworkUsages;
        this.networkUsageUtilizations = this.networkUsages.map(this._networkUsageUtilization.bind(this));
        this.initializedGraphs.networks = false;
      } else {
        // Refresh values
        newNetworkUsages.forEach((networkUsage, i) => {
          replaceValues(this.networkUsages[i], networkUsage);
          this.networkUsageUtilizations[i] = this._networkUsageUtilization(networkUsage);
        });
      }
    }

    if (this.showGpuUsage()) {
      // Refresh GPU list if there was a driver change or NVIDIA monitoring was enabled/disabled
      if (this.gpuUsages.length !== this.sysInfo.data.gpuUsages?.length) {
        this.gpuUsages = [...this.sysInfo.data.gpuUsages ?? []];
        this.initializedGraphs.gpus = false;
      } else {
        // Refresh values
        this.sysInfo.data.gpuUsages?.forEach((gpuUsage, i) => {
          replaceValues(this.gpuUsages[i], gpuUsage);
        });
      }
    }

    // Update graphs
    if (this.showCpuUsage())
      this.cpuGraph?.addValue(this.averageCpuUsage());
    if (this.showMemoryUsage())
      this.memoryGraph?.addValue(this.memoryUsagePercentage());
    // For disks, networks and GPUs we need to update all graphs from the list
    // Their names are used to match the correct graph with the correct usage
    this.diskGraphs?.forEach((graph, i) => {
      const utilization = this.diskUsages[i].utilization;
      graph.addValue(utilization);
    });
    this.networkGraphs?.forEach((graph, i) => {
      const downloadSpeed = this.networkUsages[i].downloadSpeed;
      const uploadSpeed = this.networkUsages[i].uploadSpeed;
      graph.addValues(downloadSpeed, uploadSpeed);
    });
    this.gpuGraphs?.forEach((graph, i) => {
      const utilization = this.gpuUsages[i].utilization;
      graph.addValue(utilization);
    });

    this.checkInitialization();
  }

  /**
   * Initialize graphs with previous values
   */
  checkInitialization() {
    // Int previous CPU usage
    if (!this.initializedGraphs.cpu && this.cpuGraph) {
      this.cpuGraph?.initWithValues(this.averageCpuUsageHistory());
      this.initializedGraphs.cpu = true;
    }

    // Init previous memory usage
    if (!this.initializedGraphs.memory && this.memoryGraph) {
      this.memoryGraph?.initWithValues(this.memoryUsagePercentageHistory());
      this.initializedGraphs.memory = true;
    }

    // Init previous disk usages
    if (!this.initializedGraphs.disks && (this.diskGraphs?.length ?? 0) > 0) {
      this.diskGraphs?.forEach((graph, i) => {
        const utilizationHistory = this._diskUsageUtilizationHistory(this.diskUsages[i]);
        graph.initWithValues(utilizationHistory);
      });
      this.initializedGraphs.disks = true;
    }

    // Init previous network usages
    if (!this.initializedGraphs.networks && (this.diskGraphs?.length ?? 0) > 0) {
      this.networkGraphs?.forEach((graph, i) => {
        graph.initWithValues(
          this._networkDownloadSpeedHistory(this.networkUsages[i]),
          this._networkUploadSpeedHistory(this.networkUsages[i])
        );
      });
      this.initializedGraphs.networks = true;
    }

    // Init previous GPU usages
    if (!this.initializedGraphs.gpus && (this.gpuGraphs?.length ?? 0) > 0) {
      this.gpuGraphs?.forEach((graph, i) => {
        const utilizationHistory = this.gpuUsageUtilizationHistory(this.gpuUsages[i].name);
        graph.initWithValues(utilizationHistory);
      });
      this.initializedGraphs.gpus = true;
    }
  }

  showCpuUsage(): boolean {
    return (this.supportedFeatures?.cpuUsage && this.allowedFeatures?.cpuUsage) === true;
  }

  showMemoryUsage(): boolean {
    return (this.supportedFeatures?.memoryUsage && this.allowedFeatures?.memoryUsage) === true;
  }

  showDiskUsage(): boolean {
    return (this.supportedFeatures?.diskUsage && this.allowedFeatures?.diskUsage) === true;
  }

  showNetworkUsage(): boolean {
    return (this.supportedFeatures?.networkUsage && this.allowedFeatures?.networkUsage) === true;
  }

  showGpuUsage(): boolean {
    return ((this.supportedFeatures?.amdGpuUsage || this.supportedFeatures?.intelGpuUsage || this.supportedFeatures?.nvidiaGpuUsage) && this.allowedFeatures?.gpuUsage) === true;
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

  _diskUsageUtilizationHistory(diskUsage: DiskUsage): number[] {
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

  _networkUsageUtilization(networkUsage: NetworkUsage): number {
    if (!this.networkGraphs)
      return 0;
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

    if (!usageGraph) {
      return 0;
    }

    // Select a higher value from download and upload
    let usage = usageGraph.currentUsage;
    if (usageGraph.currentSecondaryUsage > usage) {
      usage = usageGraph.currentSecondaryUsage;
    }

    return usage * 100;
  }

  private _networkDownloadSpeedHistory(networkUsage: NetworkUsage): number[] {
    return this.sysInfo.data.networkUsagesHistory
      .filter(usages => usages != null)
      .map(usages => usages!.find(usage => usage.name === networkUsage.name)?.downloadSpeed ?? 0);
  }

  private _networkUploadSpeedHistory(networkUsage: NetworkUsage): number[] {
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
