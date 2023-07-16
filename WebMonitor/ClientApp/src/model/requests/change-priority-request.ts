import { ProcessPriorityWin } from "../process-priority";

export interface ChangePriorityRequest {
  pid: number;
  priority: ProcessPriorityWin;
}
