import { CpuInfo } from "./cpu-info";
import { DiskInfo } from "./disk-info";
import { MemoryInfo } from "./memory-info";

export interface ComputerInfo {
  hostname: string;
  currentUser: string;
  osName: string;
  osVersion: string;
  osBuild: string | null;
  cpu: CpuInfo | null;
  memory: MemoryInfo | null;
  disks: DiskInfo[] | null;
}
