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
  styleUrls: ['./process-list.component.scss'],
  encapsulation: ViewEncapsulation.None
})
export class ProcessListComponent implements AfterViewInit, OnDestroy {
  headers = ["pid", "name", "cpuUsage", "memoryUsage"];
  dataSource = new MatTableDataSource<ProcessInfo>([]);
  filters = [ProcessFilter.PID, ProcessFilter.Name];
  selectedFilter = ProcessFilter.Name;
  filterValue = "";

  @ViewChild(MatSort) sort!: MatSort;
  numberHelpers = numberHelpers;

  private readonly refreshSubscription: Subscription;

  constructor(public sysInfo: SysInfoService) {
    // Load initial value
    if (sysInfo.data.processInfos != null) {
      this.dataSource.data = sysInfo.data.processInfos;
    }

    // Refresh on each tick
    this.refreshSubscription = sysInfo.onRefresh.subscribe(() => {
      if (sysInfo.data.processInfos != null) {
        this.dataSource.data = sysInfo.data.processInfos;
      }
    });
  }

  ngOnDestroy(): void {
    this.refreshSubscription.unsubscribe();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  onFilterSelectionChange() {
    this.dataSource.filterPredicate = (data: ProcessInfo, filter: string) => {
      switch (this.selectedFilter) {
        case ProcessFilter.PID:
          return data.pid.toString().includes(filter);
        case ProcessFilter.Name:
          return data.name.toLowerCase().includes(filter);
      }
    }
  }

  onFilterChange(filter: string) {
    this.dataSource.filter = filter.trim().toLowerCase();
  }

  clearFilter() {
    this.filterValue = "";
    this.onFilterChange(this.filterValue);
  }

  processFilterName(processFilter: ProcessFilter): string {
    switch (processFilter) {
      case ProcessFilter.PID:
        return "PID";
      case ProcessFilter.Name:
        return "Name";
    }
  }
}

enum ProcessFilter {
  PID,
  Name
}
