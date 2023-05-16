import { HttpClient } from '@angular/common/http';
import { Inject, Injectable } from '@angular/core';
import { Observable, firstValueFrom } from 'rxjs';
import { FileInformation, FileInformationResponse } from 'src/model/file-information';
import { FileOrDir } from 'src/model/file-or-dir';

@Injectable({
  providedIn: 'root'
})
export class FileBrowserService {
  private readonly apiUrl: string;

  constructor(
    private http: HttpClient,
    @Inject("BASE_URL") baseUrl: string
  ) {
    this.apiUrl = baseUrl + "FileBrowser/";
  }

  async getDirsAndFiles(dir?: string) {
    let requestUrl = this.apiUrl + "dir";
    // If dir is undefined, the request will be made for drive directories.d
    // If not, the request will be made for directories and files in the specified directory.
    if (dir) {
      requestUrl += `?requestedDirectory=${dir}`;
    }

    const response = await firstValueFrom(
      this.http.get<FileOrDir[]>(encodeURI(requestUrl))
    );

    return response.map(fileDir => {
      // Find last backslash (Windows) or slash (Linux)
      const lastBackslashIndex = fileDir.path.lastIndexOf("\\");
      const lastSlashIndex = fileDir.path.lastIndexOf("/");

      // On Windows backslashes and slashes can be used interchangeably,
      // but only backslashes are used when reading the file system.
      // Therefore, if there is a slash in the path the OS is not Windows and
      // we can assume that filename is located after the last slash.
      const lastSeparatorIndex = lastSlashIndex != -1 ? lastSlashIndex : lastBackslashIndex;


      // Because of the way the ASP.NET web server works, if there is a path separator
      // at the end of the path, we can assume that the path is drive letter
      // and there is no need to extract file/dir name.
      if (lastSeparatorIndex + 1 == fileDir.path.length) {
        fileDir.basename = fileDir.path;
      } else {
        fileDir.basename = fileDir.path.slice(lastSeparatorIndex + 1);
      }

      return fileDir;
    });
  }

  async getFileInfo(path: string): Promise<FileInformation> {
    const requestUrl = `${this.apiUrl}file-info?path=${path}`;

    const response = await firstValueFrom(
      this.http.get<FileInformationResponse>(encodeURI(requestUrl))
    );

    // API returns dates as strings, so we need to convert them to Date objects
    return {
      name: response.name,
      path: response.path,
      size: response.size,
      created: new Date(response.created),
      lastModified: new Date(response.lastModified),
      lastAccessed: new Date(response.lastAccessed)
    }
  }

  downloadFile(path: string) {
    window.open(`${this.apiUrl}download-file?path=${path}`, "_blank");
  }

  uploadFile(file: File, directory: string): Observable<Object> {
    const formData = new FormData();
    formData.append("file", file);

    return this.http.post(`${this.apiUrl}upload-file?path=${encodeURI(directory)}`, formData, {
      reportProgress: true,
      observe: "events"
    });
  }
}
