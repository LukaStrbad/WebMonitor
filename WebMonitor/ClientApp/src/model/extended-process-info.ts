import { ProcessPriorityWin } from "./process-priority";

export interface ExtendedProcessInfo {
  pid: number;
  name: string;
  owner: string | null;
  affinity: bigint;
  workingSet: bigint;
  peakWorkingSet: bigint;
  pagedMemory: bigint;
  peakPagedMemory: bigint;
  privateMemory: bigint;
  virtualMemory: bigint;
  peakVirtualMemory: bigint;
  threadCount: number;
  handleCount: number;
  // Windows only
  priorityWin?: ProcessPriorityWin;
}
