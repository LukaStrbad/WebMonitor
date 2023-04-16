export interface ProcessInfo {
  pid: number;
  name: string;
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
}

export type ProcessList = ProcessInfo[];
