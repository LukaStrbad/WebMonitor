export interface DiskInfo {
  diskType: string | null;
  connectionType: string | null;
  name: string;
  totalSize: bigint;
  isRemovable: boolean;
  rotationalSpeed: number | null;
}
