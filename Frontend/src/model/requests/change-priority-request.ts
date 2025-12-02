import { ProcessPriorityWin } from "../process-priority";

export interface ChangePriorityRequest {
  pid: number;
  priorityWin?: ProcessPriorityWin;
  priorityLinux?: number;
}
