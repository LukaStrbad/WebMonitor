import { AfterViewInit, Component, ElementRef, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FileOrDir } from 'src/model/file-or-dir';
import { FileBrowserService } from 'src/services/file-browser.service';
import * as numberHelpers from "src/helpers/number-helpers";
import { BreadcrumbItem } from '../components/breadcrumbs/breadcrumbs.component';
import { SysInfoService } from 'src/services/sys-info.service';
import { ComputerInfo } from "../../model/computer-info";
import { MatDialog } from '@angular/material/dialog';
import { FileDialogComponent, FileDialogData } from '../components/file-dialog/file-dialog.component';
import { Subscription, finalize } from 'rxjs';
import { HttpEventType } from '@angular/common/http';
import { FileUploadInfo } from 'src/model/file-upload-info';
import { MatSnackBar, MatSnackBarConfig } from '@angular/material/snack-bar';
import { SupportedFeatures } from "../../model/supported-features";

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
  numberOfSelectedFiles = 0;
  uploadSubscription: Subscription | undefined;
  uploadFile: { progress: number; file: File; } | undefined;
  supportedFeatures?: SupportedFeatures;

  @ViewChild(MatSort) sort!: MatSort;
  @ViewChild("fileUpload") fileUpload!: ElementRef<HTMLInputElement>;
  numberHelpers = numberHelpers;

  constructor(
    private fileBrowser: FileBrowserService,
    private sysInfo: SysInfoService,
    private dialog: MatDialog,
    private snackBar: MatSnackBar
  ) {
    this.resetBreadcrumbs();

    sysInfo.getSupportedFeatures()
      .then(supportedFeatures => this.supportedFeatures = supportedFeatures);
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
    let data: FileDialogData = {
      fileInfo: fileInfo
    }
    // Only add download function if it's supported
    if (this.supportedFeatures?.fileDownload) {
      data.download = () => this.fileBrowser.downloadFile(file.path);
    }
    const dialogRef = this.dialog.open(FileDialogComponent, { data });
  }

  uploadOnChange() {
    this.numberOfSelectedFiles = this.fileUpload.nativeElement.files?.length ?? 0;
  }

  uploadFiles() {
    if (!this.currentDir) {
      return;
    }

    this.uploadFile = {
      progress: 0,
      file: this.fileUpload.nativeElement.files![0]
    }

    let uploadSub = this.fileBrowser
      .uploadFile(this.uploadFile.file, this.currentDir)
      .pipe(finalize(() => {
        this.uploadSubscription?.unsubscribe();
        this.uploadSubscription = undefined;
        this.uploadFile = undefined;
      }));

    uploadSub.subscribe((event: any) => {
      if (event.type == HttpEventType.UploadProgress) {
        if (this.uploadFile) {
          this.uploadFile.progress = event.loaded / event.total;
        }
      } else if (event.type == HttpEventType.Response && Array.isArray(event.body)) {
        const responses = event.body as FileUploadInfo[];
        const snackBarConfig = <MatSnackBarConfig>{
          duration: 3000,
          horizontalPosition: "end",
          verticalPosition: "bottom"
        };

        if (responses[0].success) {
          this.snackBar.open("Upload successful", undefined, snackBarConfig)
          this.refreshDirsAndFiles();
        } else {
          this.snackBar.open("Upload failed", undefined, snackBarConfig)
        }
      }
    });
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
