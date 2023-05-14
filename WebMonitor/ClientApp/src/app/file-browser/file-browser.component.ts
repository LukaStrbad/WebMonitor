import { AfterViewInit, Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FileOrDir } from 'src/model/file-or-dir';
import { FileBrowserService } from 'src/services/file-browser.service';
import * as numberHelpers from "src/helpers/number-helpers";
import { BreadcrumbItem } from '../components/breadcrumbs/breadcrumbs.component';
import { SysInfoService } from 'src/services/sys-info.service';
import { ComputerInfo } from "../../model/computer-info";
import { MatDialog } from '@angular/material/dialog';
import { FileDialogComponent } from '../components/file-dialog/file-dialog.component';

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
    private sysInfo: SysInfoService,
    private dialog: MatDialog
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

    this.initAsync();
  }

  async initAsync() {
    this.setBreadcrumbSeparator(await this.sysInfo.getComputerInfo());
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
    // Change data sort function
    this.dataSource.sortData = this.tableSortFunction;
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
    let breadcrumbTitle;
    // If OS is windows and we're navigating to drive root, remove the backslash from drive letter
    if (this.breadcrumbSeparator == "\\" && this.currentDir == undefined) {
      breadcrumbTitle = dir.basename.slice(0, -1);
    } else {
      breadcrumbTitle = dir.basename;
    }

    this.currentDir = dir.path;
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
    if (row.type === "file") {
      this.onFileClick(row);
      return;
    }

    // -1 means access denied
    if (row.childrenCount === -1) {
      return;
    }

    this.navigateToDir(row);
  }

  async onFileClick(file: FileOrDir) {
    const fileInfo = await this.fileBrowser.getFileInfo(file.path);
    const data = {
      fileInfo: fileInfo,
      download: () => this.fileBrowser.downloadFile(file.path)
    }
    const dialogRef = this.dialog.open(FileDialogComponent, { data });
  }

  tableSortFunction(items: FileOrDir[], sort: MatSort): FileOrDir[] {
    return items.sort((a, b) => {
      const isAsc = sort.direction === 'asc';
      let compareValue = 0;

      switch (sort.active) {
        case "basename":
          compareValue = a.basename.localeCompare(b.basename);
          break;
        case "type":
          compareValue = a.type.localeCompare(b.type);
          break;
        case "size": {
          // Directories have lower precedence
          if (a.type === "dir" && b.type === "file") {
            compareValue = -1;
          } else if (a.type === "file" && b.type === "dir") {
            compareValue = 1;
          } else if (a.type === "dir" && b.type === "dir") {
            compareValue = (b.childrenCount ?? -1) - (a.childrenCount ?? -1);
          } else {
            compareValue = Number((a.size ?? 0n) - (b.size ?? 0n));
          }
        }
      }

      return compareValue * (isAsc ? 1 : -1);
    });
  }
}

/**
 * Breadcrumb item with stored depth
 */
interface BreadcrumbItemDepth extends BreadcrumbItem {
  depth: number;
}
