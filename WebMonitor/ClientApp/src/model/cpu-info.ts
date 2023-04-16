export interface CpuInfo {
  name: string;
  identifier: string;
  numThreads: number;
  numCores: number;
  baseFrequencies: number[];
}
