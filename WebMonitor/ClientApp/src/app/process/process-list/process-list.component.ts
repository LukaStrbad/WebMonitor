import { AfterViewInit, Component, OnDestroy, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { Subscription } from 'rxjs';
import { ProcessInfo } from 'src/model/process-info';
import { SysInfoService } from 'src/services/sys-info.service';
import * as numberHelpers from "../../../helpers/number-helpers";
import { ManagerService } from "../../../services/manager.service";
import { MatSnackBar } from "@angular/material/snack-bar";
import { showErrorSnackbar, showOkSnackbar } from "../../../helpers/snackbar-helpers";
import { ProcessDialogComponent, ProcessDialogData } from "../../components/process-dialog/process-dialog.component";
import { MatDialog, MatDialogRef } from "@angular/material/dialog";

@Component({
  selector: 'app-process-list',
  templateUrl: './process-list.component.html',
  styleUrls: ['./process-list.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class ProcessListComponent implements AfterViewInit, OnDestroy {
  headers = ["pid", "owner", "name", "cpuUsage", "memoryUsage"];
  dataSource = new MatTableDataSource<ProcessInfo>([]);
  filters = [ProcessFilter.PID, ProcessFilter.Name];
  selectedFilter = ProcessFilter.Name;
  filterValue = "";

  @ViewChild(MatSort) sort!: MatSort;
  numberHelpers = numberHelpers;

  private readonly subscriptions: Subscription[] = [];

  constructor(
    public sysInfo: SysInfoService,
    private manager: ManagerService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    // Load initial value
    if (sysInfo.data.processInfos != null) {
      this.dataSource.data = sysInfo.data.processInfos;
    }

    // Refresh on each tick
    this.subscriptions.push(sysInfo.onRefresh.subscribe(() => {
      if (sysInfo.data.processInfos != null) {
        this.dataSource.data = sysInfo.data.processInfos;
      }
    }));

    this.subscriptions.push(
      manager.okEmitter.subscribe(message => showOkSnackbar(this.snackBar, message))
    );
    this.subscriptions.push(
      manager.errorEmitter.subscribe(message => showErrorSnackbar(this.snackBar, message))
    );
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
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

  async onProcessClick(process: ProcessInfo) {
    let data: ProcessDialogData = {
      processInfo: process,
      onKill: async () => {
        await this.manager.killProcess(process.pid);
      }
    }

    this.dialog.open(ProcessDialogComponent, { data });
  }
}

enum ProcessFilter {
  PID,
  Name
}
