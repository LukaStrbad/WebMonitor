import { AfterViewInit, Component, OnDestroy, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Subscription } from 'rxjs';
import { ProcessInfo } from 'src/model/process-info';
import { SysInfoService } from 'src/services/sys-info.service';
import * as numberHelpers from "../../../helpers/number-helpers";

@Component({
  selector: 'app-process-list',
  templateUrl: './process-list.component.html',
  styleUrls: ['./process-list.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class ProcessListComponent implements AfterViewInit, OnDestroy {
  headers = ["pid", "name", "cpuUsage", "memoryUsage"];
  dataSource = new MatTableDataSource<ProcessInfo>([]);

  @ViewChild(MatSort) sort!: MatSort;
  numberHelpers = numberHelpers;

  private readonly refreshSubscription: Subscription;

  constructor(sysInfo: SysInfoService) {
    // Load initial value
    this.dataSource.data = sysInfo.data.processInfos;

    // Refresh on each tick
    this.refreshSubscription = sysInfo.onRefresh.subscribe(() => {
      this.dataSource.data = sysInfo.data.processInfos;
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }
}
