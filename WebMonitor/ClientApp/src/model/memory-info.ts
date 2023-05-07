export interface MemoryInfo {
    usableMemory: bigint;
    reservedMemory: bigint;
    totalMemory: bigint;
    speed: number;
    voltage: number;
    formFactor: string;
    type: string;
    memorySticks: MemoryStickInfo[];
}

export interface MemoryStickInfo {
    manufacturer?: string;
    partNumber?: string;
    capacity: bigint;
}
