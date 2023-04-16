export interface NetworkUsage {
  name: string;
  downloadSpeed: number;
  uploadSpeed: number;
  dataDownloaded: number;
  dataUploaded: number;
  mac: string;
}

export type NetworkUsages = NetworkUsage[];
