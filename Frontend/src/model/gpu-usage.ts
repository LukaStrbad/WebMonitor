export interface GpuUsage {
  coreClock: number;
  memoryClock: number;
  memoryTotal: bigint;
  memoryUsed: bigint;
  name: string;
  power: number | null;
  temperature: number;
  utilization: number;
  manufacturer: "NVIDIA" | "AMD" | "Intel";
}

export type GpuUsages = GpuUsage[];
