export interface ChangeAffinityRequest {
  pid: number;
  threads: ThreadInfo[];
}

export interface ThreadInfo {
  threadIndex: number;
  on: boolean;
}
