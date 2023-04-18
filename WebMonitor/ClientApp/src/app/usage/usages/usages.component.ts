import { AfterViewInit, Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { UsageGraphComponent } from "../usage-graph/usage-graph.component";
import { interval } from "rxjs";
import { SysInfoService } from "../../../services/sys-info.service";
import { NetworkUsages } from 'src/model/network-usage';
import * as arrayHelpers from "../../../helpers/array-helpers";

@Component({
  selector: 'app-usages',
  templateUrl: './usages.component.html',
  styleUrls: ['./usages.component.css']
})
export class UsagesComponent implements AfterViewInit {
  @ViewChild("cpuGraph") cpuGraph!: UsageGraphComponent;
  @ViewChild("memoryGraph") memoryGraph!: UsageGraphComponent;

  networkUsages: NetworkUsages = [];

  constructor(public sysInfo: SysInfoService) { }

  ngAfterViewInit(): void {
    this.sysInfo.onRefresh.subscribe(() => {
      this.cpuGraph.addValue(this.averageCpuUsage());
      this.memoryGraph.addValue(this.memoryUsagePercentage());
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

}
