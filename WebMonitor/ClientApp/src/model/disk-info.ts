export interface DiskInfo {
  diskType?: string;
  connectionType?: string;
  name: string;
  totalSize: bigint;
  isRemovable: boolean;
  rotationalSpeed?: number;
}
