import { AfterViewInit, Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FileOrDir } from 'src/model/file-or-dir';
import { FileBrowserService } from 'src/services/file-browser.service';
import * as numberHelpers from "src/helpers/number-helpers";
import { BreadcrumbItem } from '../components/breadcrumbs/breadcrumbs.component';
import { SysInfoService } from 'src/services/sys-info.service';
import { ComputerInfo } from "../../model/computer-info";

@Component({
  selector: 'app-file-browser',
  templateUrl: './file-browser.component.html',
  styleUrls: ['./file-browser.component.css'],
  encapsulation: ViewEncapsulation.None
})
export class FileBrowserComponent implements OnInit, AfterViewInit {
  headers = ["basename", "type", "size"];
  dataSource = new MatTableDataSource<FileOrDir>([]);
  currentDir?: string;
  depth = 0;
  /**
   * Breadcrumbs that serve as a navigation history
   */
  breadcrumbs: BreadcrumbItemDepth[] = [];
  breadcrumbSeparator = "/";

  @ViewChild(MatSort) sort!: MatSort;
  numberHelpers = numberHelpers;

  constructor(
    private fileBrowser: FileBrowserService,
    private sysInfo: SysInfoService
  ) {
    this.resetBreadcrumbs();
  }

  resetBreadcrumbs() {
    this.breadcrumbs = [{
      title: "Home",
      depth: 0,
      onClick: () => {
        this.currentDir = undefined;
        this.breadcrumbs = [];
        this.refreshDirsAndFiles();
        this.resetBreadcrumbs();
      }
    }];
  }

  ngOnInit(): void {
    this.refreshDirsAndFiles();

    if (!this.sysInfo.data.computerInfo) {
      const subscription = this.sysInfo.onRefresh.subscribe(() => {
        this.setBreadcrumbSeparator(this.sysInfo.data.computerInfo!);
        subscription.unsubscribe();
      });
    } else {
      this.setBreadcrumbSeparator(this.sysInfo.data.computerInfo);
    }
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  /**
   * Sets the breadcrumb separator based on the OS
   * @param computerInfo Computer info
   */
  setBreadcrumbSeparator(computerInfo: ComputerInfo) {
    if (computerInfo.osName.toLowerCase().includes("windows")) {
      this.breadcrumbSeparator = "\\";
    }
  }

  async refreshDirsAndFiles() {
    this.dataSource.data = await this.fileBrowser.getDirsAndFiles(this.currentDir);
  }

  /**
   * Navigates forward in the directory tree
   * @param dir Directory to navigate to
   */
  navigateToDir(dir: FileOrDir) {
    this.currentDir = dir.path;

    let breadcrumbTitle;
    // If OS is windows, remove the backslash from drive letter
    if (this.breadcrumbSeparator == "\\") {
      breadcrumbTitle = dir.basename.slice(0, -1);
    } else {
      breadcrumbTitle = dir.basename;
    }
    // Increase depth
    this.depth++;
    // Save current depth to use in onClick
    const currentDepth = this.depth;

    this.breadcrumbs.push({
      title: breadcrumbTitle,
      depth: this.depth,
      onClick: () => {
        // Go back to the clicked directory
        this.currentDir = dir.path;
        // Remove breadcrumbs that are deeper than the clicked one
        this.breadcrumbs = this.breadcrumbs.filter(b => b.depth <= currentDepth);
        // Update depth
        this.depth = currentDepth;
        this.refreshDirsAndFiles();
      }
    });

    this.refreshDirsAndFiles();
  }

  onRowClick(row: FileOrDir) {
    if (row.type !== "dir") {
      return;
    }

    // -1 means access denied
    if (row.childrenCount === -1) {
      return;
    }

    this.navigateToDir(row);
  }
}

/**
 * Breadcrumb item with stored depth
 */
interface BreadcrumbItemDepth extends BreadcrumbItem {
  depth: number;
}
