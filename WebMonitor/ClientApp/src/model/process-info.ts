export interface ProcessInfo {
  pid: number;
  owner: string | null;
  user: string;
  name: string;
  cpuUsage: number;
  memoryUsage: number;
  diskUsage: number;
}

export type ProcessList = ProcessInfo[];
