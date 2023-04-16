export interface DiskUsage {
  name: string;
  readSpeed: number;
  writeSpeed: number;
  utilization: number;
}

export type DiskUsages = DiskUsage[];
