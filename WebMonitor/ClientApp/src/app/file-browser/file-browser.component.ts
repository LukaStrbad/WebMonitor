import { AfterViewInit, Component, OnInit, ViewChild, ViewEncapsulation } from '@angular/core';
import { MatSort } from '@angular/material/sort';
import { MatTableDataSource } from '@angular/material/table';
import { FileOrDir } from 'src/model/file-or-dir';
import { FileBrowserService } from 'src/services/file-browser.service';
import * as numberHelpers from "src/helpers/number-helpers";

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

  @ViewChild(MatSort) sort!: MatSort;
  numberHelpers = numberHelpers;

  constructor(private fileBrowser: FileBrowserService) { }

  ngOnInit(): void {
    this.refreshDirsAndFiles();
  }

  ngAfterViewInit(): void {
    this.dataSource.sort = this.sort;
  }

  async refreshDirsAndFiles() {
    this.dataSource.data = await this.fileBrowser.getDirsAndFiles(this.currentDir);
  }

  navigateToDir(dir: FileOrDir) {
    this.currentDir = dir.path;
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
